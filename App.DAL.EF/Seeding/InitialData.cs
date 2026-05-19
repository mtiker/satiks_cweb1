namespace App.DAL.EF.Seeding;

public static class InitialData
{
    public static readonly string[] Roles = ["admin", "user", "technician"];

    public static readonly (string email, string password, string firstName, string lastName, string[] roles)[] Users =
        [
            ("admin@labtrack.ee",        "Admin.12345",   "Admin",      "User",       ["admin"]),
            ("tech@labtrack.ee",         "Tech.12345",    "Technician", "User",       ["technician", "user"]),
            ("technician@labrent.com",   "Technician1!",  "Lab",        "Technician", ["technician"]),
            ("user@labtrack.ee",         "User.12345",    "Regular",    "User",       ["user"])
        ];
}
