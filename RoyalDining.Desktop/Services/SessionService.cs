using RoyalDining.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RoyalDining.Desktop.Services;

public static class SessionService
{
    public static User? CurrentUser { get; private set; }

    public static bool IsLoggedIn => CurrentUser != null;
    public static bool IsAdmin => CurrentUser?.Role == "Admin";
    public static bool IsCustomer => CurrentUser?.Role == "Customer";

    public static WindowState LastWindowState { get; set; } = WindowState.Normal;

    public static bool IsLastWindowMaximized { get; set; } = false;

    public static void Login(User user)
    {
        CurrentUser = user;
    }

    public static void Logout()
    {
        CurrentUser = null;
    }
}
