﻿using EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DI
{
    public static class ProgramExtensions
    {
        //Create extension method in common layer for create database if DB not exit
        //Moved it to the common layer becasue avoid PL to direct access DAL
        public static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {   //DB initialization 
                    var context = services.GetRequiredService<PopulationAndHouseholdDataContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    //TODO: Need to handle logging later
                    //var logger = services.GetRequiredService<ILogger<Program>>();
                    //logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }
    }
}
