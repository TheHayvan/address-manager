using Address_Manager.Window;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Address_Manager.Helper;

namespace Address_Manager
{
    public partial class MainWindow : System.Windows.Window
    {
        public static addressmanagerEntities AddressmanagerEntities = new addressmanagerEntities();

        public static List<ColumnInfo> DatabaseAddressesColumns = new List<ColumnInfo>
        {
            new ColumnInfo { Name = "Vorname", DataType = typeof(string) },
            new ColumnInfo { Name = "Name", DataType = typeof(string) },
            new ColumnInfo { Name = "Firma", DataType = typeof(string) },
            new ColumnInfo { Name = "Strasse", DataType = typeof(string) },
            new ColumnInfo { Name = "Hausnummer", DataType = typeof(string) }
        };

        public static List<ColumnInfo> DatabaseLocationsColumns = new List<ColumnInfo>
        {
            new ColumnInfo { Name = "Postleitzahl", DataType = typeof(int) },
            new ColumnInfo { Name = "Ortschaft", DataType = typeof(string) }
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Open File Dialog to select CSV file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Select a CSV file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TextBoxCsvPath.Text = openFileDialog.FileName;

                // Open the validation window
                ValidateColumns validateWindow = new ValidateColumns(TextBoxCsvPath.Text);
            
                if (validateWindow.ShowDialog() == true)
                {
                    // Proceed with further processing if validation is successful
                    PopulateDataGrid(validateWindow.MappedDatabaseObjects);
                    ButtonSaveToDb.IsEnabled = true;
                }
            }
        }


        private void PopulateDataGrid(List<Addresses> validateWindowMappedDatabaseObjects)
        {
        

            DataGridRecords.ItemsSource = validateWindowMappedDatabaseObjects;

            // Highlight invalid rows
            foreach (var item in DataGridRecords.Items)
            {
                if (item is Addresses address && string.IsNullOrWhiteSpace(address.Hausnummer))
                {
                    var row = (DataGridRow)DataGridRecords.ItemContainerGenerator.ContainerFromItem(item);
                    if (row != null)
                    {
                        row.Background = Brushes.Red;
                    }
                }
            }
        }

        private void ButtonSaveToDb_Click(object sender, RoutedEventArgs e)
        {
            // Check and highlight invalid rows
        /*    foreach (var item in DataGridRecords.Items)
            {
                if (item is Addresses address && string.IsNullOrWhiteSpace(address.Hausnummer))
                {
                    MessageBox.Show("Please fix the invalid values before saving to the database.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }*/

            // Ensure distinct locations
            EnsureDistinctLocations(DataGridRecords.Items.Cast<Addresses>().ToList());

            // Perform save to the database
            SaveToDatabase(DataGridRecords.Items.Cast<Addresses>().ToList());
            MessageBox.Show("Data successfully saved to the database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EnsureDistinctLocations(List<Addresses> addresses)
        {
            foreach (var address in addresses)
            {
                if (address.Locations != null)
                {
                    var postleitzahl = address.Locations.Postleitzahl;
                    var ortschaft = address.Locations.Ortschaft;

                    // Check if the location already exists in the database
                    var existingLocation = AddressmanagerEntities.Locations
                        .FirstOrDefault(l => l.Postleitzahl == postleitzahl && l.Ortschaft == ortschaft);

                    if (existingLocation != null)
                    {
                        // Use the existing location
                        address.Locations = existingLocation;
                    }
                    else
                    {
                        // Ensure that the new location is not added multiple times
                        var newLocation = AddressmanagerEntities.Locations.Local
                            .FirstOrDefault(l => l.Postleitzahl == postleitzahl && l.Ortschaft == ortschaft);

                        if (newLocation == null)
                        {
                            // Add the new location to the context
                            AddressmanagerEntities.Locations.Add(address.Locations);
                        }
                    }
                }
            }
            AddressmanagerEntities.SaveChanges();
        }

        private void SaveToDatabase(List<Addresses> addresses)
        {
            foreach (var address in addresses)
            {
                if (address.Locations != null)
                {
                    // Check if the location already exists in the database
                    var existingLocation = AddressmanagerEntities.Locations
                        .FirstOrDefault(l => l.Postleitzahl == address.Locations.Postleitzahl
                                             && l.Ortschaft == address.Locations.Ortschaft);

                    if (existingLocation != null)
                    {
                        // Use the existing location and set LocationID
                        address.LocationID = existingLocation.LocationID;
                    }
                    else
                    {
                        // Add the new location to the context and save changes to generate LocationID
                        AddressmanagerEntities.Locations.Add(address.Locations);
                        AddressmanagerEntities.SaveChanges(); // Save immediately to get the new LocationID

                        // Set the new LocationID in the address object
                        address.LocationID = address.Locations.LocationID;
                    }

                    // Clear the Locations object to avoid redundancy
                    address.Locations = null;
                }

                // Add the address to the context
                AddressmanagerEntities.Addresses.Add(address);
            }

            // Save all addresses to the database
            AddressmanagerEntities.SaveChanges();
        }

    }

}
