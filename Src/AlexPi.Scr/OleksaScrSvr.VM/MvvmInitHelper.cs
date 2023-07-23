namespace OleksaScrSvr.VM;
public static class MvvmInitHelper
{
  public static void InitMVVM(IServiceCollection services)
  {
    _ = services.AddSingleton<ZeroStore>();
    _ = services.AddSingleton<AcntStore>();
    _ = services.AddSingleton<SrvrStore>();
    _ = services.AddSingleton<DtBsStore>();
    _ = services.AddSingleton<UserStore>();
    _ = services.AddSingleton<NavigationStore>();
    _ = services.AddSingleton<ModalNavgnStore>();
    _ = services.AddSingleton<AllowSaveStore>();
    _ = services.AddSingleton<IsBusyStore>();

    //tu: Start Page Startup Page controller.
    _ = DevOps.IsDbg
      ?              /**/    services.AddSingleton<INavSvc, Page01MultiUnitNavSvc>()
      : Environment.MachineName switch
      {
        "YOGA1" or "NUC2" => services.AddSingleton<INavSvc, Page02SlideshowNavSvc>(),
        "RAZER1"     /**/ => services.AddSingleton<INavSvc, Page01MultiUnitNavSvc>(),
        _            /**/ => services.AddSingleton<INavSvc, Page03RazerScSvNavSvc>(),
      };

    _ = services.AddSingleton<ICompositeNavSvc, CompositeNavSvc>();

    _ = services.AddSingleton<ClickOnceUpdaterNavSvc>();
    _ = services.AddSingleton<Page01MultiUnitNavSvc>();
    _ = services.AddSingleton<Page02SlideshowNavSvc>();
    _ = services.AddSingleton<Page03RazerScSvNavSvc>();
    _ = services.AddSingleton<AcntNavSvc>();
    _ = services.AddSingleton<ZeroNavSvc>();
    _ = services.AddSingleton<UserListingNavSvc>();
    _ = services.AddSingleton<SrvrListingNavSvc>();
    _ = services.AddSingleton<DtBsListingNavSvc>();
    _ = services.AddTransient<LoginPopupMdlNavSvc>();
    _ = services.AddTransient<LoginCloseMdlNavSvs>();
    _ = services.AddTransient<AddSrvrMdlNavSvc>();
    _ = services.AddTransient<AddDtBsMdlNavSvc>();
    _ = services.AddTransient<AddUserMdlNavSvc>();
    _ = services.AddSingleton<CloseModalNavSvc>();

    _ = services.AddSingleton(s => new Func<NavBarVM>(() => s.GetRequiredService<NavBarVM>()!));
    _ = services.AddSingleton(s => new Func<AcntVM>(() => s.GetRequiredService<AcntVM>()!));
    _ = services.AddSingleton(s => new Func<ZeroVM>(() => s.GetRequiredService<ZeroVM>()!));
    _ = services.AddSingleton(s => new Func<LoginVM>(() => s.GetRequiredService<LoginVM>()!));
    _ = services.AddSingleton(s => new Func<UserListingVM>(() => s.GetRequiredService<UserListingVM>()!));
    _ = services.AddSingleton(s => new Func<SrvrListingVM>(() => s.GetRequiredService<SrvrListingVM>()!));
    _ = services.AddSingleton(s => new Func<DtBsListingVM>(() => s.GetRequiredService<DtBsListingVM>()!));
    _ = services.AddSingleton(s => new Func<AddSrvrVM>(() => s.GetRequiredService<AddSrvrVM>()!));
    _ = services.AddSingleton(s => new Func<AddDtBsVM>(() => s.GetRequiredService<AddDtBsVM>()!));
    _ = services.AddSingleton(s => new Func<AddUserVM>(() => s.GetRequiredService<AddUserVM>()!));
    _ = services.AddSingleton(s => new Func<ClickOnceUpdaterVM>(() => s.GetRequiredService<ClickOnceUpdaterVM>()!));
    _ = services.AddSingleton(s => new Func<Page01MultiUnitVM>(() => s.GetRequiredService<Page01MultiUnitVM>()!));
    _ = services.AddSingleton(s => new Func<Page02SlideshowVM>(() => s.GetRequiredService<Page02SlideshowVM>()!));
    _ = services.AddSingleton(s => new Func<Page03RazerScSvVM>(() => s.GetRequiredService<Page03RazerScSvVM>()!));

    _ = services.AddTransient<NavBarVM>();
    _ = services.AddSingleton<MainVM>();
    _ = services.AddTransient<AcntVM>();
    _ = services.AddTransient<ZeroVM>();
    _ = services.AddTransient<LoginVM>();
    _ = services.AddTransient<SrvrListingVM>();
    _ = services.AddTransient<DtBsListingVM>();
    _ = services.AddTransient<UserListingVM>();
    _ = services.AddTransient<AddSrvrVM>();
    _ = services.AddTransient<AddDtBsVM>();
    _ = services.AddTransient<AddUserVM>();
    _ = services.AddTransient<ClickOnceUpdaterVM>();
    _ = services.AddTransient<Page03RazerScSvVM>();
    _ = services.AddTransient<Page01MultiUnitVM>();
    _ = services.AddTransient<Page02SlideshowVM>();

    _ = services.AddTransient<ISecForcer, /*SecurityEnforcement.Mok.*/SecForcer>(); // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    _ = services.AddTransient<SqlPermissionsManager>();
    _ = services.AddTransient<BmsPermissionsManager>();

    _ = services.AddTransient<UserSettingsSPM>();
  }
}

public class SecForcer : ISecForcer // SecurityEnforcement.Mok. ~~~~~~~~~~~~~~~        ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
{
  public SecForcer() : this(true, true) { }
  public SecForcer(bool isRead, bool isEdit) => (CanRead, CanEdit) = (isRead, isEdit);

  public bool CanRead { get; }
  public bool CanEdit { get; }

  public string PermisssionCSV => $"{(CanRead ? "Read+" : "")}{(CanEdit ? "Edit+" : "")}".Trim(new[] { '+', ' ' });

  public bool HasAccessTo(PermissionFlag ownedPermissions, PermissionFlag resource) => ownedPermissions.HasFlag(resource);
}
