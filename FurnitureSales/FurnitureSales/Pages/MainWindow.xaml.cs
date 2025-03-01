﻿using ClosedXML.Excel;
using FurnitureSales.Models;
using FurnitureSales.Pages;
using iText.StyledXmlParser.Jsoup.Nodes;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FurnitureSales
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string name = string.Empty;
        FurnitureDBEntities db = new FurnitureDBEntities();
        private int Index;
        public MainWindow()
        {
            InitializeComponent();
            RefreshgGrid(); 
            Global.cureWindow = this;
            Global.cureGrid = contractsDataGrid;

            BitmapImage photo = new BitmapImage();
            photo.BeginInit();

            photo.UriSource = new Uri($"D:\\VS_PROJECTS\\Furniture-Sales-\\FurnitureSales\\FurnitureSales\\Resources\\{Global.User.typeOfAccount}.jpg");
            photo.EndInit();
            profilePhoto.Source = photo;

            profileName.Content = Global.userName;
            profileType.Content = Global.User.typeOfAccount;


            if (Global.User.typeOfAccount == "Organization")
            {
                WorkersTI.Visibility = Visibility.Hidden;
                SalesTI.Visibility = Visibility.Hidden;
                BuyersTI.Visibility = Visibility.Hidden;
                BuyerPanel.Visibility = Visibility.Visible;
            }
            else if (Global.User.typeOfAccount == "Manager")
            {
                WorkersTI.Visibility = Visibility.Hidden;
                BuyersTI.Visibility = Visibility.Hidden;
                ManagerPanel.Visibility = Visibility.Visible;
                BtnPDF.Visibility = Visibility.Visible;
                BtnExcel.Visibility = Visibility.Visible;
            }
            else
            {
                AdminPanel.Visibility = Visibility.Visible;
            }

        }
        //go back to autorization 
        private void butBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Global.Autorization.Show();
            //Global.cureWindow.Close();

        }

        //closing window
        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);


        }


        //refreshing contractsGrid
        private void RefreshgGrid()
        {
            db = new FurnitureDBEntities();
            db.Contarcts.Load();
            if (Global.User.typeOfAccount == "Organization")
            {
                contractsDataGrid.ItemsSource = db.Contarcts.Local.Select(x => new
                {
                    ID = x.idContract,
                    Organization = (from org in db.Buyers where org.idBuyer == x.idBuyer select org.nameOfOrganization).First(),
                    RegisterDate = x.registrationDate.ToString("dd/MM/yyyy"),
                    DueDate = x.dueDate.ToString("dd/MM/yyyy"),
                    Order = Orders(x.order)
                }).Where(x => x.Organization == Global.userName).ToList();
            }
            else
            {
                contractsDataGrid.ItemsSource = db.Contarcts.Local.Select(x => new
                {
                    ID = x.idContract,
                    Organization = (from org in db.Buyers where org.idBuyer == x.idBuyer select org.nameOfOrganization).First(),
                    RegisterDate = x.registrationDate.ToString("dd/MM/yyyy"),
                    DueDate = x.dueDate.ToString("dd/MM/yyyy"),
                    Order = Orders(x.order)
                }).ToList();
            }


            contractsDataGrid.Items.Refresh();
        }

        //set orders for all contracts
        private string Orders(int id)
        {
            string res = "";
            var order = from contract in db.ContractsSales where contract.idContract == id select contract;
            foreach (var i in order)
            {
                res += $"TYPE: {i.TypeOfFurniture} COUNT: {i.count}\n";
            }
            return res;
        }


        //multisorting click
        private void butSort_Click(object sender, RoutedEventArgs e)
        {

            Global.colunsNames.Clear();
            foreach (var item in Global.cureGrid.Columns)
            {
                Global.colunsNames.Add(item.Header.ToString());
            }

            SortingWindow sortingWindow = new SortingWindow();
            sortingWindow.Show();




        }
        //new contract
        private void butAddNewContract_Click(object sender, RoutedEventArgs e)
        {
            Global.TableState = "New";
            CreateUpdateWindow createWindow = new CreateUpdateWindow(name);
            createWindow.Show();

        }


        //delete contracts
        private void butDelContract_Click(object sender, RoutedEventArgs e)
        {
            //where i can write switch for deleting in every table 
            var result = MessageBox.Show(Index.ToString(), $"Delete {name}", MessageBoxButton.OKCancel);


            if (result == MessageBoxResult.OK)
            {
                try
                {
                    switch (name)
                    {
                        case "Contracts":
                            var contract = (from table in db.Contarcts where table.idContract == Index select table).First();
                            var values = from table in db.ContractsSales where table.idContract == Index select table;
                            foreach (var item in values)
                            {
                                db.ContractsSales.Remove(item);
                            }
                            db.Contarcts.Remove(contract);
                            break;

                        case "Workers":
                            var worker = (from table in db.Workers where table.idWorker == Index select table).First();
                            db.Workers.Remove(worker);
                            var account = (from table in db.Accounts where table.idAccount == worker.codeAccount select table).First();
                            db.Accounts.Remove(account);
                            break;
                        case "Buyers":
                            var buyer = (from table in db.Buyers where table.idBuyer == Index select table).First();
                            db.Buyers.Remove(buyer);
                            break;

                    }
                    
                    db.SaveChanges();
                    MessageBox.Show("DELETED");
                    RefreshgGrid();
                }
                catch
                {
                    MessageBox.Show("ERR");
                }
            }
        }
        //refresh click
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            contractsDataGrid.Items.Refresh();
            RefreshgGrid();
        }







        public static DataGridCell GetCell(DataGrid grid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(grid, row);
            return GetCell(grid, rowContainer, column);
        }

        public static DataGridRow GetRow(DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // May be virtualized, bring into view and try again.
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }
        public static DataGridCell GetCell(DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        private static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }













        //double click choise row 
        private void DataGridRow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Index = Global.cureGrid.SelectedIndex + 1;
            if(name == "Models")
            {
                var cell = GetCell(modelsDataGrid, Index - 1, 0).Content as TextBlock;
                Global.ModelName = cell.Text;
            }

        }

        //udapte contracts
        private void butUpdateContract_Click(object sender, RoutedEventArgs e)
        {
            Global.Index = Index;
            Global.TableState = "Update";
            CreateUpdateWindow createWindow = new CreateUpdateWindow(name);
            createWindow.Show();
        }
        private void UpdateTables_Click(object sender, RoutedEventArgs e)
        {
            UpdateTables();
        }
        private void UpdateTables()
        {
            db = new FurnitureDBEntities();
            switch (name)
            {

                case "Contracts":
                    Global.cureGrid = contractsDataGrid;
                    butDel.IsEnabled = true;
                    butAddNew.IsEnabled = true;
                    butUpdate.IsEnabled = true;
                    break;

                case "Models":
                    db.TypesOfFurnitures.Load();
                    modelsDataGrid.ItemsSource = db.TypesOfFurnitures.Local.Select(x => new
                    {
                        Model = x.model,
                        Name = x.furnitureName,
                        Sizes = x.furnitureCharacteristics,
                        Price = x.price.ToString() + " $"

                    }).ToList();
                    butDel.IsEnabled = false;
                    butUpdate.IsEnabled = true;
                    butAddNew.IsEnabled = true;
                    Global.cureGrid = modelsDataGrid;
                    break;

                case "Sales":
                    db.Sales.Load();
                    SalesDataGrid.ItemsSource = db.Sales.Local.Select(x => new
                    {
                        ID = x.idSale,
                        Name = x.furnitureName,
                        Model = x.model,
                        SoldCount = x.numberOfSold

                    }).ToList();
                    Global.cureGrid = SalesDataGrid;
                    butDel.IsEnabled = false;
                    butAddNew.IsEnabled = false;
                    butUpdate.IsEnabled = true;
                    break;

                case "Workers":
                    db.Workers.Load();
                    workersDataGrid.ItemsSource = db.Workers.Local.Select(x => new
                    {
                        ID = x.idWorker,
                        Surname = x.surname,
                        Name = x.name,
                        Patronymic = x.patronymic,
                        Post = x.post,
                        Login = (from login in db.Accounts where login.idAccount == x.codeAccount select login.login).First(),
                        Password = (from password in db.Accounts where password.idAccount == x.codeAccount select password.password).First(),
                    }).ToList();
                    Global.cureGrid = workersDataGrid;
                    butDel.IsEnabled = true;
                    butUpdate.IsEnabled = true;
                    butAddNew.IsEnabled = true;
                    break;

                case "Buyers":
                    db.Buyers.Load();
                    buyersDataGrid.ItemsSource = db.Buyers.Local.Select(x => new
                    {
                        ID = x.idBuyer,
                        Organization = x.nameOfOrganization,
                        Adress = x.adress,
                        Phone = x.phone,
                        Login = (from login in db.Accounts where login.idAccount == x.codeAccount select login.login).First(),
                        Password = (from password in db.Accounts where password.idAccount == x.codeAccount select password.password).First(),
                    }).ToList();
                    Global.cureGrid = buyersDataGrid;
                    butDel.IsEnabled = true;
                    butUpdate.IsEnabled = true;
                    butAddNew.IsEnabled = true;
                    break;
            }
        }
        //change tables
        private void TabCCC_GotFocus(object sender, RoutedEventArgs e)
        {
            //change datagrid
            TabItem ti = TabCCC.SelectedItem as TabItem;
            if (name != ti.Header.ToString())
            {
                
                name = ti.Header.ToString();
                UpdateTables();
                
            }
        }


        private void ButSearch_Click(object sender, RoutedEventArgs e)
        {
            switch (name)
            {

                case "Contracts":
                    Global.cureGrid = contractsDataGrid;
                    butDel.IsEnabled = true;
                    butAddNew.IsEnabled = true;
                    butUpdate.IsEnabled = true;
                    break;

                case "Models":
                    db.TypesOfFurnitures.Load();
                    modelsDataGrid.ItemsSource = db.TypesOfFurnitures.Local.Select(x => new
                    {
                        Model = x.model,
                        Name = x.furnitureName,
                        Sizes = x.furnitureCharacteristics,
                        Price = x.price.ToString() + " $"

                    }).Where(y => y.Model.Contains(SearchString.Text)).ToList();

                    butDel.IsEnabled = false;
                    butUpdate.IsEnabled = true;
                    butAddNew.IsEnabled = true;
                    Global.cureGrid = modelsDataGrid;
                    break;

                case "Sales":
                    db.Sales.Load();
                    SalesDataGrid.ItemsSource = db.Sales.Local.Select(x => new
                    {
                        ID = x.idSale,
                        Name = x.furnitureName,
                        Model = x.model,
                        SoldCount = x.numberOfSold

                    }).Where(y => y.Model.Contains(SearchString.Text)).ToList();
                    Global.cureGrid = SalesDataGrid;
                    butDel.IsEnabled = false;
                    butAddNew.IsEnabled = false;
                    butUpdate.IsEnabled = true;
                    break;

                case "Workers":
                    db.Workers.Load();
                    workersDataGrid.ItemsSource = db.Workers.Local.Select(x => new
                    {
                        ID = x.idWorker,
                        Surname = x.surname,
                        Name = x.name,
                        Patronymic = x.patronymic,
                        Post = x.post,
                        Login = (from login in db.Accounts where login.idAccount == x.codeAccount select login.login).First(),
                        Password = (from password in db.Accounts where password.idAccount == x.codeAccount select password.password).First(),
                    }).Where(y => y.Surname.Contains(SearchString.Text)).ToList();
                    Global.cureGrid = workersDataGrid;
                    butDel.IsEnabled = true;
                    butUpdate.IsEnabled = true;
                    butAddNew.IsEnabled = true;
                    break;

                case "Buyers":
                    db.Buyers.Load();
                    buyersDataGrid.ItemsSource = db.Buyers.Local.Select(x => new
                    {
                        ID = x.idBuyer,
                        Organization = x.nameOfOrganization,
                        Adress = x.adress,
                        Phone = x.phone,
                        Login = (from login in db.Accounts where login.idAccount == x.codeAccount select login.login).First(),
                        Password = (from password in db.Accounts where password.idAccount == x.codeAccount select password.password).First(),
                    }).Where(y => y.Organization.Contains(SearchString.Text)).ToList();
                    Global.cureGrid = buyersDataGrid;
                    butDel.IsEnabled = true;
                    butUpdate.IsEnabled = true;
                    butAddNew.IsEnabled = true;
                    break;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GraphicWindow graphicWindow = new GraphicWindow();
            graphicWindow.Show();
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            List<string> cols = new List<string>();


            for (int i = 0; i < SalesDataGrid.Columns.Select(a => a.Header).Count(); i++)
            {
                cols.Add(SalesDataGrid.Columns[i].Header.ToString());
            }


            using (XLWorkbook workbook = new XLWorkbook())
            {
                try
                {

                    var worksheet = workbook.Worksheets.Add("Sales");


                    worksheet.Column(1).Width = 5;
                    worksheet.Column(2).Width = 13;
                    worksheet.Column(3).Width = 29;

                    worksheet.Column(4).Width = 19;
                    worksheet.Row(1).Style.Font.Bold = true;
                    worksheet.Row(1).Style.Font.FontSize = 18;

                    worksheet.Rows(2, 100).Style.Font.FontSize = 16;
                    worksheet.Rows(1, 100).Style.Font.FontName = "TimesNewRoman";


                    var sales = from s in db.Sales select s;

                    for (int k = 0; k < cols.Count; k++)
                    {
                        worksheet.Cell(1, k + 1).Value = cols[k];
                    }

                    int i = 2;
                    foreach (var item in sales)
                    {
                        worksheet.Cell(i, 1).Value = item.idSale;
                        worksheet.Cell(i, 2).Value = item.furnitureName;
                        worksheet.Cell(i, 3).Value = item.model;
                        worksheet.Cell(i, 4).Value = item.numberOfSold;

                        i++;
                    }
                    SaveFileDialog fileDialog = new SaveFileDialog();
                    fileDialog.ShowDialog();
                    workbook.SaveAs(fileDialog.FileName);
                }
                catch
                {
                    MessageBox.Show("Err");
                }
            }
        }

        private void BtnPDF_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files | *.pdf";
            saveFileDialog.DefaultExt = "pdf";
            saveFileDialog.ShowDialog();


            iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A2, 10f, 10f, 10f, 0f);

            PdfWriter.GetInstance(pdfDoc, new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate));
            pdfDoc.Open();
            PdfPTable pdfTable = new PdfPTable(SalesDataGrid.Columns.Count);
            pdfTable.WidthPercentage = 100;
            pdfTable.DefaultCell.Padding = 3;
            pdfTable.DefaultCell.BorderWidth = 1;
            pdfTable.DefaultCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

            // Add the DataGrid's columns to the table
            for (int i = 0; i < SalesDataGrid.Columns.Count; i++)
            {
                //DataGridTextColumn column = InsuranceTable.Columns[i] as DataGridTextColumn;
                pdfTable.AddCell(SalesDataGrid.Columns[i].Header.ToString());
            }

            //Add the DataGrid's rows to the table
            for (int i = 0; i < SalesDataGrid.Items.Count; i++)
            {
                for (int j = 0; j < SalesDataGrid.Columns.Count; j++)
                {
                    var cell = GetCell(SalesDataGrid, i, j).Content as TextBlock;
                    pdfTable.AddCell(new Phrase(cell.Text));
                }
            }

            //Add the table to the PDF document
            //pdfDoc.Add(new iTextSharp.text.Paragraph("Hello World!"));
            pdfDoc.Add(pdfTable);


            //// Close the PDF document
            pdfDoc.Close();
        }
    }
}
