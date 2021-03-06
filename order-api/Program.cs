﻿using System;
using Microsoft.Owin.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {
        var url = "http://+:8081";

        using (WebApp.Start<Startup>(url))
        {
            Console.WriteLine("Running on {0}", url);
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
