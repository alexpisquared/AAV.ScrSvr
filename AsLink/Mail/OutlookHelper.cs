using AAV.Sys.Helpers;
using AAV.WPF.Ext;
using AgentFastAdmin;
using AsLink;
using AvailStatusEmailer.Helpers;
using Db.QStats.DbModel;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OL = Microsoft.Office.Interop.Outlook;

namespace AvailStatusEmailer.View
{
  class Misc { public const string qRcvd = "Q", qSent = "Sent Items", qSentDone = "Sent Items/_DbDoneSent", qDltd = "Deleted Items", qFail = "Q/Fails", qRcvdDone = "Q/_DbDoneRcvd", qLate = "Q/ToReSend"/*, qVOld = "Q/VeryOld"*/; }

  public class OutlookHelper
  {
    readonly OL.Application _olApp;
    readonly OL.MAPIFolder _contactsFolder;
    readonly int _customLetersSentThreshold = 3; // to become an Outlook contact, must have at least 3 letters sent.
    static readonly char[] _delim = new[] { ' ', '.', ',', ':', ';', '\r', '\n', '\'', '"', '_' };
    int _updatedCount, _addedCount;
    const string _regexEmailPattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"; // //var r = new Regex(@"/\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}\b/");         \b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b  <== http://www.regular-expressions.info/email.html

    public OutlookHelper()
    {
      try
      {
        _olApp = new OL.Application();

        MyStore = _olApp.Session.Stores[AAV.Sys.Helpers.VerHelper.IsMyHomePC ? "alex.pigida@outlook.com" : "apigida@nymi.com"];
        _contactsFolder = MyStore.GetDefaultFolder(OL.OlDefaultFolders.olFolderContacts);        // this.Application.GetNamespace("MAPI").GetDefaultFolder(OL.OlDefaultFolders.olFolderContacts);                //_deletedsFolder = _store.GetDefaultFolder(OL.OlDefaultFolders.olFolderDeletedItems);
      }
      catch (COMException ex) { ex.Pop("I think this is it... (ap: Jun`20)"); }
      catch (Exception ex) { ex.Pop(); throw; }
    }

    public OL.Store MyStore { get; }
    public OL.Items GetItemsFromFolder(string folder, int old)
    {
      try
      {
        var folder0 = MyStore.GetRootFolder().Folders[folder] as OL.Folder;
        var items = folder0.Items.Restrict("[MessageClass] = 'IPM.Note'");
        return items;
      }
      catch (Exception ex) { ex.Pop(folder); throw; }
    }
    public OL.Items GetDeliveryFailedItems()
    {
      try
      {
        var folder = MyStore.GetRootFolder().Folders[Misc.qRcvd].Folders[@"Fails"] as OL.Folder;
        var itemss = folder.Items.Restrict("[MessageClass] = 'REPORT.IPM.Note.NDR'");
        Debug.WriteLine($"***        Fails: {itemss.Count}");
        return itemss;
      }
      catch (Exception ex) { ex.Pop(@"Q\Fails"); throw; }
    }
    public OL.Items GetItemsFromFolder(string folderPath, string messageClass = null) // IPM.Note, REPORT.IPM.Note.NDR
    {
      try
      {
        var folder = GetMapiFOlder(folderPath);

        var itemss = messageClass == null ? folder.Items : folder.Items.Restrict($"[MessageClass] = '{messageClass}'");
        //...Debug.WriteLine($" *** {folderPath,24}: {itemss.Count}");
        return itemss;
      }
      catch (Exception ex) { ex.Pop(@"Q\Fails"); throw; }
    }
    public OL.Items GetToResendItems()
    {
      try
      {
        var folder = MyStore.GetRootFolder().Folders[Misc.qRcvd].Folders[@"ToReSend"] as OL.Folder;
        var itemss = folder.Items.Restrict("[MessageClass] = 'IPM.Note'");
        return itemss;
      }
      catch (Exception ex) { ex.Pop(@"Q\ToReSend"); throw; }
    }
    public OL.MAPIFolder GetMapiFOlder(string folderPath)
    {
      var folderParts = folderPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
      var folder = MyStore.GetRootFolder();
      for (var i = 0; i < folderParts.Length; i++)
      {
        folder = folder.Folders[folderParts[i]] as OL.Folder;
      }

      return folder;
    }

    public async Task<string> OutlookUndeleteContactsAsync(A0DbContext db)
    {
      App.SpeakAsync("Synchronous action... usually takes 5 minutes.");

      var sw = Stopwatch.StartNew();
      db.EMails.Load();
      db.Agencies.Load();

      undeleteContacts(MyStore.GetDefaultFolder(OL.OlDefaultFolders.olFolderContacts), db, "Contacts");
      undeleteContacts(MyStore.GetDefaultFolder(OL.OlDefaultFolders.olFolderDeletedItems), db, "Deleted Items");

      App.SpeakAsync($"All done. Took {sw.Elapsed.TotalMinutes:N1} minutes.");

      var rep =      //db.TrySaveReport(); // 
        db.GetDbChangesReport();
      Debug.WriteLine($"{rep}");

      Task<string> saveAndUpdateMetadata()
      {
        return AgentAdminnWindow.SaveAndUpdateMetadata(db);
      }

      await AgentFastAdmin.AgentAdminnWindow.CheckAskToSaveDispose_CanditdteForGlobalRepltAsync(db, false, saveAndUpdateMetadata);

      return rep;
    }


    public void FindContactByName(string lastName)
    {
      try
      {
        var i = (OL.ContactItem)_contactsFolder.Items.Find(string.Format("[LastName]='{0}'", lastName)); // ..Format("[FirstName]='{0}' and [LastName]='{1}'", firstName, lastName));
        while (i != null)
        {
          i.Display(true);
          i = _contactsFolder.Items.FindNext();
        }
      }
      catch (Exception ex) { ex.Pop(lastName); throw; }
    }
    public void FindContactByEmail(string email)
    {
      try
      {
        //var contact = (OL.ContactItem)contactsFolder.Items.Find(String.Format("[Email1Address]='{0}'", email.ToUpper())); // ..Format("[FirstName]='{0}' and [LastName]='{1}'", firstName, lastName));
        //while (contact != null)
        //{
        //  contact.Display(true);
        //  contact = contactsFolder.Items.FindNext();
        //}

        dbgListAllCOntacts(MyStore.GetDefaultFolder(OL.OlDefaultFolders.olFolderContacts));
        dbgListAllCOntacts(MyStore.GetDefaultFolder(OL.OlDefaultFolders.olFolderDeletedItems));
      }
      catch (Exception ex) { ex.Pop("!!! MUST RUN OUTLOOK TO WORK !!!"); throw; }
    }
    public string SyncDbToOutlook(A0DbContext db, string folderName = null)
    {
      AAV.Sys.Helpers.Bpr.Beep1of2();
      var q = db.EMails.Where(r => string.IsNullOrEmpty(r.PermBanReason)
                              && r.ID.Contains("@")
                              && !r.ID.Contains("=")
                              && !r.ID.Contains("reply")
                              && !r.ID.Contains("'")
                              && !r.ID.Contains("+")
                              && r.EHists.Count(e => string.Compare(e.RecivedOrSent, "S", true) == 0 && !string.IsNullOrEmpty(e.LetterBody) && e.LetterBody.Length > 96) > _customLetersSentThreshold); //at least 2 letters sent (1 could be just an unanswered reply on their broadcast)

      var ttl = q.Count();
      Debug.WriteLine("\r\n{0} eligible email contacts found", ttl);
      _addedCount = _updatedCount = 0;
      foreach (var em in q)
      {
        AddUpdateOutlookContact(em);
      }

      Bpr.Beep2of2();
      return string.Format("\r\nTotal Outlook: {0} added, {1} updated / out of {2} eligibles. \r\n", _addedCount, _updatedCount, ttl);
    }
    public void AddUpdateOutlookContact(EMail em)
    {
      try
      {
        var i = (OL.ContactItem)_contactsFolder.Items.Find(string.Format("[Email1Address]='{0}' or [Email2Address]='{0}' or [Email3Address]='{0}' ", em.ID));
        if (i != null)
        {
          mergeDbDataToOutlookContact(em, ref i, "by Email");
          return;
        }

        if (!string.IsNullOrWhiteSpace(em.FName) && !string.IsNullOrWhiteSpace(em.LName) && _contactsFolder.Items.Find(string.Format("[FirstName]='{0}' and [LastName]='{1}'", em.FName, em.LName)) != null)
        {
          i = (OL.ContactItem)_contactsFolder.Items.Find(string.Format("[FirstName]='{0}' and [LastName]='{1}'", em.FName, em.LName));
          if (i != null)
          {
            mergeDbDataToOutlookContact(em, ref i, "by Name");
            return;
          }
        }

        createFromDbOutlookContact(em);
      }
      catch (Exception ex) { ex.Pop("!!! MUST RUN OUTLOOK TO WORK !!!"); throw; }
    }




    void mergeDbDataToOutlookContact(EMail em, ref OL.ContactItem i, string msg)
    {
      bool changed;
      while (i != null)
      {
        Debug.WriteLine("\r\nUpdating outlook: {0,12} {1,-12} - {2,32} - Body: {3} \r\n    with DB data: {4,12} {5,-12} - {6,32} - Body: {7}  ", i.FirstName, i.LastName, i.Email1Address, i.Body, em.FName, em.LName, em.ID, em.Notes);
        changed = false;


        if (!string.IsNullOrWhiteSpace(em.Notes))
        {
          if (string.IsNullOrWhiteSpace(i.Body))
          {
            i.Body = em.Notes;
            changed = true;
          }
          else if (!i.Body.Contains(em.Notes))
          {
            i.Body += (Environment.NewLine + Environment.NewLine + " -=-=- From QStats DB -=-=- [[" + Environment.NewLine + em.Notes + Environment.NewLine + "]] -=-=- From QStats DB -=-=-");
            changed = true;
          }
        }

        if (string.IsNullOrWhiteSpace(i.Email1Address))                                                                                                                  /**/{ i.Email1Address = em.ID; changed = true; }
        else if (string.Compare(i.Email1Address, em.ID, true) != 0 && string.IsNullOrWhiteSpace(i.Email2Address))                                                       /**/{ i.Email2Address = em.ID; changed = true; }
        else if (string.Compare(i.Email1Address, em.ID, true) != 0 && string.Compare(i.Email2Address, em.ID, true) != 0 && string.IsNullOrWhiteSpace(i.Email3Address)) /**/{ i.Email3Address = em.ID; changed = true; }
        else if (string.Compare(i.Email1Address, em.ID, true) != 0 && string.Compare(i.Email2Address, em.ID, true) != 0 && string.Compare(i.Email3Address, em.ID, true) != 0)
        { i.Body += (Environment.NewLine + Environment.NewLine + em.ID); changed = true; }

        if (string.IsNullOrWhiteSpace(i.FirstName) && !string.IsNullOrWhiteSpace(em.FName))                  /**/ { changed = true; i.FirstName = em.FName; }
        if (string.IsNullOrWhiteSpace(i.LastName) && !string.IsNullOrWhiteSpace(em.LName))                   /**/ { changed = true; i.LastName = em.LName; }
        if (string.IsNullOrWhiteSpace(i.CompanyName) && !string.IsNullOrWhiteSpace(em.Company))              /**/ { changed = true; i.CompanyName = em.Company; }
        if (string.IsNullOrWhiteSpace(i.BusinessTelephoneNumber) && !string.IsNullOrWhiteSpace(em.Phone))    /**/ { changed = true; i.BusinessTelephoneNumber = em.Phone; }

        if (changed)
        {
          if (string.IsNullOrWhiteSpace(i.Categories)) i.Categories = "AppAdded";
          i.User2 = msg;
          i.Save();
          _updatedCount++;
        }

        i = _contactsFolder.Items.FindNext();
      }
    }
    void createFromDbOutlookContact(EMail em)
    {
      var i = (OL.ContactItem)_olApp.CreateItem(OL.OlItemType.olContactItem);
      i.Email1Address = em.ID;
      if (!string.IsNullOrWhiteSpace(em.FName))    /**/ i.FirstName = em.FName;
      if (!string.IsNullOrWhiteSpace(em.LName))    /**/ i.LastName = em.LName;
      if (!string.IsNullOrWhiteSpace(em.Company))  /**/ i.CompanyName = em.Company;
      if (!string.IsNullOrWhiteSpace(em.Phone))    /**/ i.BusinessTelephoneNumber = em.Phone;

      var q = em.EHists.Where(e => string.Compare(e.RecivedOrSent, "S", true) == 0 && !string.IsNullOrWhiteSpace(e.LetterBody) && e.LetterBody.Length > 96);
      var m = q.Max(e => e.EmailedAt);
      var t = q.FirstOrDefault(r => r.EmailedAt == m);
      i.Body = " -=-=- [[From QStats DB (brandNew): -=-=- " + Environment.NewLine + (!string.IsNullOrWhiteSpace(em.Notes) ? em.Notes : "") +
        string.Format("{0}Added: {1:yyyy-MM-dd},  as of {2:yyyy-MM-dd} hand-written letters sent: {3},  {0}  first: {4:yyyy-MM-dd}  - last: {5:yyyy-MM-dd}:{0}{0}{6}{0} -=-=- EOFrom QStasts DB]] -=-=- {0}",
        Environment.NewLine,
        em.AddedAt,
        DateTime.Today,
        q.Count(),
        q.Min(e => e.EmailedAt),
        q.Max(e => e.EmailedAt),
        t == null ? "" : (t.LetterBody.Length > 222 ? (t.LetterBody.Substring(0, 222) + " ...") : t.LetterBody));

      //i.Display(true);

      i.Categories = "AppAdded";
      i.User1 = "New from QStats DB";
      //i.CreationTime = AvailStatusEmailer.App.Now;

      i.Save();
      _addedCount++;
    }
    void dbgListAllCOntacts(OL.MAPIFolder folder)
    {
      Debug.WriteLine($"Folder {folder.Name} has total {folder.Items.Count} items: ");

      foreach (var o in folder.Items) //.Where(r => r==r))
      {
        if (o is OL.ContactItem)          /**/{ var i = o as OL.ContactItem; Debug.WriteLine($"C {i.FirstName,16}\t{i.LastName,-16}\t{i.Email1Address,24}\t{i.Subject,-48}\t{(string.IsNullOrWhiteSpace(i.Body) ? "·" : (i.Body.Length > 50 ? i.Body.Substring(0, 50) : i.Body))}\t{i.Account}"); }
        else if (o is OL.MailItem)        /**/{ var i = o as OL.MailItem; Debug.WriteLine($"M {i.To,-32}\t{i.Subject,-48}\t"); }
        else if (o is OL.AppointmentItem) /**/{ var i = o as OL.AppointmentItem; Debug.WriteLine($"M {i.Subject,-48}\t"); }
        else if (o is OL.MeetingItem)     /**/{ var i = o as OL.MeetingItem; Debug.WriteLine($"M {i.Subject,-48}\t"); }
        else if (o is OL.TaskItem)        /**/{ var i = o as OL.TaskItem; Debug.WriteLine($"M {i.Body,-48}\t"); }
        else
        {
          foreach (PropertyDescriptor descrip in TypeDescriptor.GetProperties(o))
          {
            Debug.Write($" {descrip.Name}"); // if (descrip.Name == "Subject") { foreach (PropertyDescriptor descrip2 in TypeDescriptor.GetProperties(descrip)) { if (descrip2.Name == "sub attribute Name") { } } }
          }
          Debug.Write($"\n");
        }
      }
    }
    void undeleteContacts(OL.MAPIFolder folder, A0DbContext a0DbContext, string srcFolder)
    {
      Debug.WriteLine($"Folder {folder.Name} has total {folder.Items.Count} items: ");

      foreach (var o in folder.Items)
      {
        if (o is OL.ContactItem)
        {
          addUpdateToDb(o as OL.ContactItem, a0DbContext, srcFolder);
        }
      }
    }

    void addUpdateToDb(OL.ContactItem ci, A0DbContext db, string srcFolder)
    {
      const int maxLen = 256;

      var emailId = "";
      if (!string.IsNullOrWhiteSpace(ci.Email1Address))                                               /**/{ emailId = ci.Email1Address; }
      else if (!string.IsNullOrWhiteSpace(ci.Email2Address))                                          /**/{ emailId = ci.Email2Address; }
      else if (!string.IsNullOrWhiteSpace(ci.Email3Address))                                          /**/{ emailId = ci.Email3Address; }
      else if (!string.IsNullOrWhiteSpace(ci.FirstName) && !string.IsNullOrWhiteSpace(ci.LastName))   /**/{ emailId = $"{ci.FirstName}.{ci.LastName}@__UnKnwn__.com"; }
      else if (!string.IsNullOrWhiteSpace(ci.FirstName))                                              /**/{ emailId = $"{ci.FirstName}.__UnKnwn__@__UnKnwn__.com"; }
      else if (!string.IsNullOrWhiteSpace(ci.LastName))                                               /**/{ emailId = $"__UnKnwn__.{ci.LastName}@__UnKnwn__.com"; }
      else
      {
        Debug.WriteLine($"******************");
        return;
      }

      if (!string.IsNullOrWhiteSpace(ci.Body)) Debug.WriteLine($"{(string.IsNullOrWhiteSpace(ci.Body) ? "·" : (ci.Body.Length > 50 ? ci.Body.Substring(0, 50) : ci.Body))}"); // <= strange thing: all bodies are empty.

      var phone = $"{ci.HomeTelephoneNumber} {ci.PrimaryTelephoneNumber} {ci.BusinessTelephoneNumber} {ci.Business2TelephoneNumber} {ci.MobileTelephoneNumber}".Replace("(", "").Replace(")", "-").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();
      var agency = /*!string.IsNullOrWhiteSpace(ci.CompanyName) ? ci.CompanyName : */GetCompanyName(emailId);

      if (!db.Agencies.Local.Any(r => agency.Equals(r.ID, StringComparison.OrdinalIgnoreCase)))
        db.Agencies.Local.Add(new Agency
        {
          ID = agency.Length > maxLen ? agency.Substring(agency.Length - maxLen, maxLen) : agency,
          AddedAt = App.Now
        });


      Debug.WriteLine($"{emailId,32}\t{ci.FirstName,17} {ci.LastName,-21}\t{ci.JobTitle,-80}\t{phone}\t{(string.IsNullOrWhiteSpace(ci.Body) ? "·" : (ci.Body.Length > 50 ? ci.Body.Substring(0, 50) : ci.Body))}");

      addUpdateBassedOnGoodEmailId(ci, db, emailId, phone, agency, srcFolder);
    }
    public static string GetCompanyName(string email)
    {
      var indexofAt = email.IndexOf('@') + 1;
      var indexofEx = email.LastIndexOf('.');
      var len = indexofEx - indexofAt;

      if (len < 1)
        return email;

      var cmpny = email.Substring(indexofAt, len);
      return cmpny;
    }

    void addUpdateBassedOnGoodEmailId(OL.ContactItem ci, A0DbContext db, string emailId, string phone, string agency, string srcFolder)
    {
      var an = $"-={srcFolder}-Add=-{ci.JobTitle}·{ci.Body}¦";
      var em = db.EMails.Local.FirstOrDefault(r => emailId.Equals(r.ID, StringComparison.OrdinalIgnoreCase));
      if (em == null)
      {
        var e0 = new EMail
        {
          ID = emailId, // $"__UnKnwn__:{Guid.NewGuid()}",
          FName = ci.FirstName,
          LName = ci.LastName,
          //Company = agency,
          Phone = phone,
          Notes = an,
          AddedAt = App.Now
        };
        db.EMails.Local.Add(e0);
      }
      else
      {
        var note = $"-={srcFolder}-Upd=-{ci.JobTitle}·{ci.Email2Address} {ci.Email3Address}·{phone}·{ci.Body}¦";

        if (string.IsNullOrWhiteSpace(em.Phone) && !string.IsNullOrWhiteSpace(phone))
        {
          em.Phone = phone;
          em.ModifiedAt = App.Now;
        }

        if (string.IsNullOrWhiteSpace(em.Notes))
        {
          em.Notes = note;
          em.ModifiedAt = App.Now;
        }
        else if (!string.IsNullOrWhiteSpace(em.Notes))
        {
          if (!string.IsNullOrWhiteSpace(ci.JobTitle      /**/) && !em.Notes.Contains(ci.JobTitle      /**/)) { em.ModifiedAt = App.Now; em.Notes += $" + {ci.JobTitle}"; }
          if (!string.IsNullOrWhiteSpace(ci.Body          /**/) && !em.Notes.Contains(ci.Body          /**/)) { em.ModifiedAt = App.Now; em.Notes += $" + {ci.Body}"; }
        }
        else
        {
          return;
        }
      }
    }


    public static string figureOutSenderEmail(OL.MailItem mailItem) => !string.IsNullOrEmpty(mailItem.Sender?.Address) && mailItem.Sender.Address.Contains("@") ? mailItem.Sender.Address :
                        !string.IsNullOrEmpty(mailItem.SenderEmailAddress) && mailItem.SenderEmailAddress.Contains("=") && mailItem.SenderEmailAddress.Contains("@") ? RemoveBadEmailParts(mailItem.SenderEmailAddress) :
                        !string.IsNullOrEmpty(mailItem.SenderEmailAddress) && mailItem.SenderEmailAddress.Contains("@") ? mailItem.SenderEmailAddress :
                        !string.IsNullOrEmpty(mailItem.SentOnBehalfOfName) && mailItem.SentOnBehalfOfName.Contains("@") ? mailItem.SentOnBehalfOfName :
                        mailItem.SenderEmailAddress;
    public static (string, string) figureOutSenderFLName(OL.MailItem mailItem, string email)
    {
      var fln =
        !string.IsNullOrEmpty(mailItem.Sender?.Name) && mailItem.Sender.Name.Contains(" ") ? mailItem.Sender.Name :
        !string.IsNullOrEmpty(mailItem.SentOnBehalfOfName) && mailItem.SentOnBehalfOfName.Contains(" ") ? mailItem.SentOnBehalfOfName :
        !string.IsNullOrEmpty(mailItem.Sender?.Name) ? mailItem.Sender.Name :
        !string.IsNullOrEmpty(mailItem.SentOnBehalfOfName) ? mailItem.SentOnBehalfOfName :
        null;

      return figureOutSenderFLName(fln, email);
    }

    internal static bool ValidEmailAddress(string emailaddress)
    {
      try
      {
        if (
          emailaddress.Length > 48 ||
          emailaddress.Contains('\\') ||
          emailaddress.Contains('/') ||
          emailaddress.Contains('=')
          ) return false;

        var m = new MailAddress(emailaddress);
        return true;
      }
      catch (FormatException)
      {
        return false;
      }
    }

    public static (string, string) figureOutSenderFLName(string fln, string email)
    {
      if (fln == null ||
        fln == "Marketing- SharedMB" // randstad on behalf of case
        )
      {
        var hlp = new FirstLastNameParser(email);
        return (hlp.FirstName, hlp.LastName);
      }

      if (fln.Contains("via Indeed"))
        return ("Sirs", "");

      fln = fln.Trim(_delim);

      var idx = fln.IndexOf('@');
      if (idx > 0)
        fln = fln.Substring(0, idx);

      var flnArray = fln.Split(_delim, StringSplitOptions.RemoveEmptyEntries);
      if (flnArray.Length == 1)
        return (flnArray[0], "");

      if (flnArray.Length >= 2)
      {
        if (fln.Contains(','))
          return (flnArray[1], flnArray[0]);
        else
          return (flnArray[0], flnArray[1]);
      }

      return ("Sirs", "");
    }
    public static (string, string) figureOutFLNameFromBody(string body, string email)
    {
      body = body.ToLower();
      email = email.ToLower();
      var idx = body.IndexOf(email);
      var words = body.Substring(0, idx).Split(_delim, StringSplitOptions.RemoveEmptyEntries);
      var len = words.Length;
      if (len > 5)
      {
        Debug.Write($">>> {words[len - 5]} {words[len - 4]}    {words[len - 3]} {words[len - 2]}    {words[len - 1]}    <= '{email}' ");

        if (new[] { "at", "-", "@", "<" }.Contains(words[len - 1]))
        {
          if (new[] { "contact", "manager", "email", "to", "from", "com>", "ca>", "ca", "com" }.Contains(words[len - 4]))
            return (words[len - 3], words[len - 2]);           // Thank you for your email. Liam Tang is no longer with Experis-Veritaaq, please contact Michael Baraban at michaelb@experis-veritaaq.ca.
        }
        else
        if (words[len - 1] == "(")
        {
          if (words[len - 4] == "contact") return (words[len - 3], words[len - 2]);            // ... contact Gary Shearer (gary.shearer@appcentrica.com <mailto:gary.shearer@appcentrica.com> ). 
        }
        else
        if (new[] { "<mailto", "addresses" }.Contains(words[len - 1]))
        {
          Debug.WriteLine($" ignore this !!!");
        }
        else
        if (Debugger.IsAttached)
          Debug.WriteLine($" ignore this ???");
      }

      var hlp = new FirstLastNameParser(email);
      return (hlp.FirstName, hlp.LastName);
    }
    public static (string, string) figureOutSenderFLName(OL.ReportItem reportItem, string email)
    {
      var pc = reportItem.Body.ToLower().IndexOf("please contact");
      if (pc > 0)
      {
        var words = reportItem.Body.Substring(startIndex: pc).Split(new[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
        return (words[2], words[3]);
      }

      var hlp = new FirstLastNameParser(email);
      return (hlp.FirstName, hlp.LastName);
    }
    public static string reportLine(string folder, string senderEmail, bool isNew) => $"{folder,-15}{(isNew ? "*" : " ")} {senderEmail,-64}\n";
    public static string reportSectionTtl(string folder, int ttl, int isNew) => ($"{folder,-15}  =>  total/new: {ttl,3} / {isNew} \n\n");
    public static string reportSectionTtl(string folder, int ttl, int ttlBanned, int newEmailsAdded) => ($"{folder,-15}  =>  total/banned/new: {ttl,3} / {ttlBanned} / {newEmailsAdded} \n\n");
    public static string RemoveBadEmailParts(string emailAddress)
    {
      emailAddress = emailAddress.Trim(_delim); //  new[] { ' ', '\'', '`', ';', ':' });

      foreach (var delim in new char[] { '=', '?' }) emailAddress = removeBadEmailParts(emailAddress, delim);

      return emailAddress;
    }
    static string removeBadEmailParts(string emailAddress, char delim)
    {
      var a = emailAddress.IndexOf(delim);
      if (a < 0) return emailAddress;

      var b = emailAddress.IndexOf('@');
      if (b > 0 && a < b)
      {
        var c = emailAddress.Substring(a, b - a);
        var d = emailAddress.Replace(c, "");
        return d;
      }
      else
      {
        var c = emailAddress.Substring(0, a);
        return c;
      }
    }
    public static void moveIt(OL.MAPIFolder targetFolder, OL.MailItem ol_item)
    {
#if DEBUG
#else
      ol_item.Move(targetFolder);
#endif
    }
    public static void moveIt(OL.MAPIFolder targetFolder, OL.ReportItem ol_item)
    {
#if DEBUG
#else
      ol_item.Move(targetFolder);
#endif
    }
    public static void test(OL.ReportItem item, string ww)
    {
      try
      {
        Debug.Write($"==> {ww}: ");

        var pa = item.PropertyAccessor;
        object prop = pa.GetProperty($"http://schemas.microsoft.com/mapi/proptag/{ww}");

        if (prop is byte[])
        {
          var snd = pa.BinaryToString(prop);
          Debug.Write($"=== snd: {snd}");

          var s = item.Session.GetAddressEntryFromID(snd);
          Debug.Write($"*** GetExchangeDistributionList(): {s.GetExchangeDistributionList()}"); //.PrimarySmtpAddress;
          Debug.Write($"*** Address: {s.Address}"); //.PrimarySmtpAddress;
        }
        //else if (sndr is byte[]) { }
        else
        {
          Debug.Write($"==> {prop.GetType().Name}  {prop}");
        }

      }
      catch (Exception ex) { Debug.Write($"!!! {ex.Message}"); }

      Debug.WriteLine($"^^^^^^^^^^^^^^^^^^^^^^^^^");
    }
    public static string getStringBetween(string b, string s1, string s2)
    {
      var i1 = b.IndexOf(s1);
      if (i1 < 0) return null;
      i1 += s1.Length;
      var i2 = b.IndexOf(s2, i1);
      if (i2 < 0) return null;
      if (i2 < i1) return null;
      var em = b.Substring(i1, i2 - i1);
      return em;
    }

    public static string[] findEmails_OLD(string body)
    {
      var email = getStringBetween(body, "Recipient(s):\r\n\t<", ">\r\n");
      if (email == null) email = getStringBetween(body, "Recipient(s):\r\n\t", "\r\n");
      if (email == null) email = getStringBetween(body, "To: ", "\r\n");
      if (email == null) email = getStringBetween(body, "<", ">");
      //if (email == null) email = item.SentOnBehalfOfName;
      //if (email == null) continue;
      //if (!email.Contains("@")) email = item.SenderEmailAddress;

      if (email == null || !email.Contains("@"))
      {
        var matches = new Regex(_regexEmailPattern, RegexOptions.IgnoreCase).Matches(body);
        if (matches.Count > 0)
        {
          var emails = new string[matches.Count];
          for (var i = 0; i < matches.Count; i++) emails[i] = matches[i].Value;
          return emails;
        }
      }

      if (email == null || !email.Contains("@")) return new string[0];

      if (!email.Contains("@")) Debug.Write("");
      if (email.Contains("<")) email = email.Replace("<", "");
      if (email.Contains(">")) email = email.Replace(">", "");
      if (email.Contains(" ")) email = email.Split(' ')[0];
      if (email.Contains(":")) email = email.Split(':')[1];
      if (email.Contains("\"")) email = email.Trim('"');

      email = email.Trim();

      return new string[] { email.Trim() };
    }
    public static string[] findEmails(string body)
    {
      var matches = new Regex(_regexEmailPattern, RegexOptions.IgnoreCase).Matches(body);
      var emails = new string[matches.Count];
      for (var i = 0; i < matches.Count; i++) emails[i] = matches[i].Value;
      return emails;
    }
  }
}
