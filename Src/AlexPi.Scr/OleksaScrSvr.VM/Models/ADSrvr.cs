namespace OleksaScrSvr.Models;

public record ADSrvr(string Name, string Description, string Note = "The Same");
public record ADDtBs(string Name, string Description, string Note, DateTime CreatedAt);
public record ADRole(string Name, string Description, string Note = "The Same");
