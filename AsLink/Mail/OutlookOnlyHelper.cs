using AAV.Sys.Ext;
using AAV.Sys.Helpers;
using AsLink;
using System;
using System.Runtime.InteropServices;
using OLk = Microsoft.Office.Interop.Outlook;

namespace AvailStatusEmailer.View
{
  class Misc { public const string Inbox = "Inbox", qRcvd = "Q", qSent = "Sent Items", qSentDone = "Sent Items/_DbDoneSent", qDltd = "Deleted Items", qFail = "Q/Fails", qRcvdDone = "Q/_DbDoneRcvd", qLate = "Q/ToReSend"/*, qVOld = "Q/VeryOld"*/; }

  public class OutlookHelper
  {
    static readonly char[] _delim = new[] { ' ', '.', ',', ':', ';', '\r', '\n', '\'', '"', '_' };
    const string _regexEmailPattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"; // //var r = new Regex(@"/\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}\b/");         \b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b  <== http://www.regular-expressions.info/email.html
    readonly OLk.Store _store; //  { get; }

    public OutlookHelper()
    {
      try
      {
        _store = new OLk.Application().Session.Stores[VerHelper.IsMyHomePC ? "alex.pigida@outlook.com" : "alex.pigida@iress.com"];        // OLk.MAPIFolder _contactsFolder = _store.GetDefaultFolder(OLk.OlDefaultFolders.olFolderContacts);        // this.Application.GetNamespace("MAPI").GetDefaultFolder(OL.OlDefaultFolders.olFolderContacts);                //_deletedsFolder = _store.GetDefaultFolder(OL.OlDefaultFolders.olFolderDeletedItems);
      }
      catch (COMException ex) { ex.Log(); }
      catch (Exception ex) { ex.Log(); throw; }
    }

    public OLk.Items GetItemsFromFolder(string folderPath, string messageClass = null) // IPM.Note, REPORT.IPM.Note.NDR
    {
      try
      {
        var folder = GetMapiFOlder(folderPath);
        var itemss = messageClass == null ? folder.Items : folder.Items.Restrict($"[MessageClass] = '{messageClass}'");         //...Debug.WriteLine($" *** {folderPath,24}: {itemss.Count}");
        return itemss;
      }
      catch (Exception ex) { ex.Log($"folder:{folderPath},  messageClass:{messageClass}"); throw; }
    }

    [Obsolete("looks like it works only when Outlook is up and running")]
    public static OLk.Items GetItemsFromInbox()
    {
      var _olApp = new OLk.Application();

      var ae = _olApp.ActiveExplorer();

      if (ae == null) foreach (OLk.Explorer exp in _olApp.Explorers) ae = exp;

      var df = ae.Session.GetDefaultFolder(OLk.OlDefaultFolders.olFolderInbox); // session is null here when outlook is not running

      return df.Items;
    }


    OLk.MAPIFolder GetMapiFOlder(string folderPath)
    {
      var folderParts = folderPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
      var outputFolder = _store.GetRootFolder();

      for (var i = 0; i < folderParts.Length; i++)
      {
        try { outputFolder = outputFolder.Folders[folderParts[i]] as OLk.Folder; } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"___Ignoring for folders like Q, etc: '{folderParts[i]}'  ({ex.Message})"); }
      }

      return outputFolder;
    }
  }
}