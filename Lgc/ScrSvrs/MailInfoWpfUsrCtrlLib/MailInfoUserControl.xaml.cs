using AAV.Sys.Ext;
using AsLink;
using AvailStatusEmailer.View;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using olk = Microsoft.Office.Interop.Outlook;

namespace MailInfoWpfUsrCtrlLib
{
  public partial class MailInfoUserControl : UserControl
  {
    //static void doSmth() { Debugger.Break(); }
    readonly int _durBack, _durWait;
    readonly Storyboard _sbWait, _sbBack;
    readonly OutlookHelper _oh = new OutlookHelper();
    //olk.Application outlookApp;
    olk.NameSpace _outlookNmSpace;
    ArrayList _unreadMails;

    public MailInfoUserControl()
    {
      try
      {
        InitializeComponent(); //btnRunItemCount_Click();            //ThisAddIn_Startup__2();            //ff();            //fff(); 

        _sbWait = (FindResource("sbMoveSecondHand") as Storyboard);
        _sbBack = (FindResource("sbMoveSecondBack") as Storyboard);

        _durBack = (int)((Duration)FindResource("durBack")).TimeSpan.TotalMilliseconds;
#if DEBUG
      _durWait = 5000;
#else
        _durWait = (int)((Duration)FindResource("durWait")).TimeSpan.TotalMilliseconds;
#endif

        Loaded += onLoaded;
      }
      catch (System.Exception ex)
      {
        ex.Log("Happens only at Zoe's PC");
      }
    }
    void onLoaded(object s, RoutedEventArgs e) => new DispatcherTimer(TimeSpan.FromMilliseconds(_durWait), DispatcherPriority.Background, new EventHandler(async (ss, ee) => await checkEmail()), Dispatcher.CurrentDispatcher).Start(); //tu: one-line timer
    async Task checkEmail()
    {
      try
      {
        arc2.BeginStoryboard(_sbBack);

        var lettersFromQ_dir = _oh.GetItemsFromFolder(Misc.qRcvd);
        var lettersFromInbox = _oh.GetItemsFromFolder(Misc.Inbox);

        var oldMailCountI = lettersFromInbox.Count;
        var newMailCountI = lettersFromInbox.Restrict("[Unread] = true").Count; //http://msdn.microsoft.com/en-us/library/ms268760(v=vs.100).aspx
        var oldMailCountQ = lettersFromQ_dir.Count;
        var newMailCountQ = lettersFromQ_dir.Restrict("[Unread] = true").Count; //http://msdn.microsoft.com/en-us/library/ms268760(v=vs.100).aspx

        tbNewMailCount.Text = $"{newMailCountI + newMailCountQ}";
        tbOldMailCount.Text = $"{oldMailCountI + oldMailCountQ}";

        imgUnkn.Visibility = Visibility.Collapsed;

        if ((newMailCountI + newMailCountQ) > 0) { imgUnkn.Visibility = Visibility.Collapsed; imgXNew.Visibility = Visibility.Visible; }
        else /*                               */ { imgUnkn.Visibility = Visibility.Visible; imgXNew.Visibility = Visibility.Collapsed; }

        await Task.Delay(_durBack);
      }
      catch (System.Exception ex)
      {
        ex.Log();
        tbNewMailCount.Text = ex.Message.Substring(0, 12);
        tbNewMailCount.FontSize = 16;
        imgUnkn.Visibility = Visibility.Visible;
        img0New.Visibility = Visibility.Collapsed;
        imgXNew.Visibility = Visibility.Collapsed;
      }
      finally
      {
        _sbBack.Stop();
        arc2.BeginStoryboard(_sbWait);
      }
    }

    //http://social.msdn.microsoft.com/Forums/en-AU/vsto/thread/e58ff668-ed55-4618-bccc-ca04a881edce
    #region Run Item Count //http://stackoverflow.com/questions/8016322/how-does-one-get-the-inbox-folder-and-item-count
    void btnRunItemCount_Click()//object sender, EventArgs e)
    {
      var outlookNmSpace = new olk.Application().GetNamespace("MAPI");

      var tbInbox_ItemCount___ = outlookNmSpace.GetDefaultFolder(OlDefaultFolders.olFolderInbox).Items.Count;
      var tbFolderItemCount___ = outlookNmSpace.GetDefaultFolder(OlDefaultFolders.olFolderInbox).Folders.Count;
      var tbSent_MailICount___ = outlookNmSpace.GetDefaultFolder(OlDefaultFolders.olFolderSentMail).Items.Count;
      var tbCalendarItemCount_ = outlookNmSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar).Items.Count;
      var tbContactsItemCount_ = outlookNmSpace.GetDefaultFolder(OlDefaultFolders.olFolderContacts).Items.Count;
    }
    void ThisAddIn_Startup__2()//object sender, System.EventArgs e)
    {
      try
      {
        var outlookApp = new olk.Application();
        _outlookNmSpace = outlookApp.GetNamespace("MAPI");

        outlookApp.NewMailEx += new olk.ApplicationEvents_11_NewMailExEventHandler(outlookApplication_NewMailEx);
        _unreadMails = new ArrayList();

        RefreshIcon();
      }
      catch (System.Exception ex) { ex.Log(); }
    }
    //http://social.msdn.microsoft.com/Forums/en/vsto/thread/ccaa7cd7-89cc-4422-a323-dc4a6942be89
    void ff()
    {

      // Don't do this in an AddIn !!!!!
      // Use Globals.ThisAddIn.Application and the existing Session instead
      // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
      var olApp = new olk.Application();
      var nameSpace = olApp.GetNamespace("MAPI");
      // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

      olk.MAPIFolder inboxFolder = null;
      olk.Items unreadItems = null;

      try
      {
        // Your default contactfolder
        inboxFolder = olApp.Session.GetDefaultFolder(olk.OlDefaultFolders.olFolderInbox);

        // Looking for Unread Items.      
        unreadItems = inboxFolder.Items.Restrict("[Unread]=true");

        // use a countdown-loop
        for (var i = unreadItems.Count; i >= 1; i--)
        {

          // get the first Item
          object item = unreadItems[1];

          // can be different types, not only mailitems...
          if (item is olk.MailItem mailItem)
          {
            Trace.WriteLine(mailItem.Subject);
            mailItem = null;
          }

          item = null;

        }

        // not in an AddIn !!!!!!!!!!!!!!!!!
        nameSpace.Logoff();

      }
      catch (System.Exception ex) { Trace.WriteLine($@"C:\c\Lgc\ScrSvrs\MailInfoWpfUsrCtrlLib\MailInfoUserControl.xaml.cs: {ex}"); }
      finally
      {
        unreadItems = null;
        inboxFolder = null;
        nameSpace = null;
        olApp = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
      }

    }

    //http://www.programminghelp.com/programming/dotnet/access-your-email-within-outlook-pt-1-of-3-c/

    //http://msdn.microsoft.com/en-us/library/bb157889.aspx

    void fff() // http://www.add-in-express.com/creating-addins-blog/2011/09/27/outlook-get-unread-items/
    {

      var outlookNmSpace = new olk.Application().GetNamespace("MAPI");

      var folderInbox = outlookNmSpace.GetDefaultFolder(olk.OlDefaultFolders.olFolderInbox);

      FindAllUnreadEmails(folderInbox);

      if (folderInbox != null) Marshal.ReleaseComObject(folderInbox);

      if (outlookNmSpace != null) Marshal.ReleaseComObject(outlookNmSpace);

    }
    #endregion

    void outlookApplication_NewMailEx(string EntryIDCollection)
    {
      olk.MailItem newMail = _outlookNmSpace.GetItemFromID(EntryIDCollection);
      newMail.Read += new olk.ItemEvents_10_ReadEventHandler(newMail_Read);
      _unreadMails.Add(newMail);
      RefreshIcon();
    }
    void newMail_Read()
    {
      //olk.MailItem justReadMail = Globals.ThisAddIn.Application.ActiveExplorer().Selection[1];
      //for (int i = 0; i < unreadMails.Count; i++)
      //{
      //  if (justReadMail.EntryID.Equals(((olk.MailItem)unreadMails[i]).EntryID))
      //  {
      //    unreadMails.RemoveAt(i);
      //    break;
      //  }
      //}
      //RefreshIcon();
    }
    void RefreshIcon()
    {
      var showIcon = false;

      showIcon = _unreadMails.Count > 0;
      try
      {

        if (showIcon)// && !iconVisible)
        {
          //TaskbarManager.Instance.SetOverlayIcon(Properties.Resources.NewMailIcon, null);
        }
        else if (!showIcon)// && iconVisible)
        {
          //TaskbarManager.Instance.SetOverlayIcon(null, null);
        }
      }
      catch (System.Exception ex) { ex.Log(); }
    }

    void FindAllUnreadEmails(olk.MAPIFolder folder)
    {
      var searchCriteria = "[UnRead] = true";
      StringBuilder strBuilder = null;
      var counter = default(int);
      olk._MailItem mail = null;
      olk.Items folderItems = null;
      object resultItem = null;
      try
      {
        if (folder.UnReadItemCount > 0)
        {
          strBuilder = new StringBuilder();
          folderItems = folder.Items;
          resultItem = folderItems.Find(searchCriteria);
          while (resultItem != null)
          {
            if (resultItem is olk._MailItem)
            {
              counter++;
              mail = resultItem as olk._MailItem;
              strBuilder.AppendLine("#" + counter.ToString() +
                                                          "\tSubject: " + mail.Subject);
            }
            Marshal.ReleaseComObject(resultItem);
            resultItem = folderItems.FindNext();
          }
          if (strBuilder != null)
            Debug.WriteLine(strBuilder.ToString());
        }
        else
          Debug.WriteLine("There is no match in the "
                                                   + folder.Name + " folder.");
      }
      catch (System.Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      finally
      {
        if (folderItems != null) Marshal.ReleaseComObject(folderItems);
      }
    }
    void FindAllUnreadEmails__2(olk.MAPIFolder folder)
    {
      var searchCriteria = "[UnRead] = true";
      StringBuilder strBuilder = null;
      var counter = default(int);
      olk._MailItem mail = null;
      olk.Items folderItems = null;
      object resultItem = null;
      try
      {
        if (folder.UnReadItemCount > 0)
        {
          strBuilder = new StringBuilder();
          folderItems = folder.Items;
          resultItem = folderItems.Find(searchCriteria);
          while (resultItem != null)
          {
            if (resultItem is olk._MailItem)
            {
              counter++;
              mail = resultItem as olk._MailItem;
              strBuilder.AppendLine("#" + counter.ToString() +
                                                          "\tSubject: " + mail.Subject);
            }
            Marshal.ReleaseComObject(resultItem);
            resultItem = folderItems.FindNext();
          }
          if (strBuilder != null)
            Debug.WriteLine(strBuilder.ToString());
        }
        else
          Debug.WriteLine("There is no match in the " +
                                                       folder.Name + " folder.");
      }
      catch (System.Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      finally
      {
        if (folderItems != null) Marshal.ReleaseComObject(folderItems);
      }
    }
  }
}

