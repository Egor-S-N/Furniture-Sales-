﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FurnitureSales.Models;

namespace FurnitureSales.Pages
{
    /// <summary>
    /// Логика взаимодействия для CreateUpdateWindow.xaml
    /// </summary>
    public partial class CreateUpdateWindow : Window
    {
        public CreateUpdateWindow(string tableName)
        {
            InitializeComponent();

            switch (tableName)
            {
                case "Contracts":
                    this.Content = new  CreateUpdateContractsPage();
                    break;

                case "Models":
                    this.Content = new CreateUpdateFurnityreTypesPage();
                    break;
                case "Sales":
                    this.Content = new UpdateSalesPage();
                    break;
                case "Workers":
                    this.Content = new CreateUpdateWorkersPage();
                    break;

                case "Buyers":
                    this.Content = new CreateUpdateBuyersPage();
                    break;
            }



            if (Global.TableState == "New")
            {
                this.Title = $"Add new {tableName}";
                //AddComboBox();

            }
            if (Global.TableState == "Update")
            {
                this.Title = $"Update current {tableName}";
            }
        }
        
    }
}
