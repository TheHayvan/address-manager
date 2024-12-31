using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CsvHelper;
using CsvHelper.Configuration;

namespace Address_Manager.Window
{
    public partial class ValidateColumns : System.Windows.Window
    {
        // List of CSV and Database columns
        private List<string> CsvColumns = new List<string>();
        private List<string> DatabaseColumns = new List<string>();
        private List<Mapping> MappedColumns = new List<Mapping>();
        private List<Mapping> IgnoredColumns = new List<Mapping>();

        public List<Addresses> MappedDatabaseObjects { get; private set; } = new List<Addresses>();

        private List<Dictionary<string, string>> csvData;

        public ValidateColumns(string csvFilePath)
        {
            InitializeComponent();

            // Load CSV columns and database columns
            CsvColumns = LoadCsvColumns(csvFilePath);
            DatabaseColumns = LoadDatabaseColumns();

            // Populate the top DataGrids
            CsvColumnsGrid.ItemsSource = CsvColumns;
            DatabaseColumnsGrid.ItemsSource = DatabaseColumns;

            UpdateUIState();
        }


        public List<string> LoadCsvColumns(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath), "CSV file path cannot be null or empty.");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified CSV file does not exist.", filePath);
            }

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture)
                {
                    HasHeaderRecord = true, // Indicates the first row contains headers
                    Delimiter = ";"
                }))
                {
                    if (csv.Read() && csv.ReadHeader())
                    {
                        csvData = csv.GetRecords<dynamic>()
                            .Select(row => ((IDictionary<string, object>)row)
                                .ToDictionary(k => k.Key, v => v.Value?.ToString()))
                            .ToList();

                        return new List<string>(csv.HeaderRecord ?? throw new InvalidOperationException()); // HeaderRecord contains all column names
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while reading the CSV file.", ex);
            }

            return new List<string>(); // Return an empty list if no headers are found
        }


        // Load database columns using the static reference
        private List<string> LoadDatabaseColumns()
        {
            return MainWindow.DatabaseAddressesColumns.Select(column => column.Name)
                .Union(MainWindow.DatabaseLocationsColumns.Select(column => column.Name))
                .ToList();
        }

        private void AddMappingButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure one item is selected from both grids
            var selectedCsvColumn = CsvColumnsGrid.SelectedItem as string;
            var selectedDatabaseColumn = DatabaseColumnsGrid.SelectedItem as string;

            if (selectedCsvColumn != null && selectedDatabaseColumn != null)
            {
                // Add to mapped columns
                MappedColumns.Add(new Mapping
                {
                    CsvColumn = selectedCsvColumn,
                    DatabaseColumn = selectedDatabaseColumn
                });

                // Remove from available lists
                CsvColumns.Remove(selectedCsvColumn);
                DatabaseColumns.Remove(selectedDatabaseColumn);

                // Refresh DataGrids
                CsvColumnsGrid.ItemsSource = null;
                CsvColumnsGrid.ItemsSource = CsvColumns;

                DatabaseColumnsGrid.ItemsSource = null;
                DatabaseColumnsGrid.ItemsSource = DatabaseColumns;

                MappedColumnsGrid.ItemsSource = null;
                MappedColumnsGrid.ItemsSource = MappedColumns;

                UpdateUIState();
            }
        }



        private void IgnoreButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCsvColumn = CsvColumnsGrid.SelectedItem as string;

            if (selectedCsvColumn != null)
            {
                // Add to ignored columns
                IgnoredColumns.Add(new Mapping
                {
                    CsvColumn = selectedCsvColumn,
                    DatabaseColumn = null
                });

                // Remove from available CSV columns
                CsvColumns.Remove(selectedCsvColumn);

                // Refresh DataGrids
                CsvColumnsGrid.ItemsSource = null;
                CsvColumnsGrid.ItemsSource = CsvColumns;

                IgnoredColumnsGrid.ItemsSource = null;
                IgnoredColumnsGrid.ItemsSource = IgnoredColumns;

                UpdateUIState();
            }
        }

        private void RemoveMappingButton_Click(object sender, RoutedEventArgs e)
        {
            // Check which grid has a selected item
            var selectedMappedItem = MappedColumnsGrid.SelectedItem as Mapping;
            var selectedIgnoredItem = IgnoredColumnsGrid.SelectedItem as Mapping;

            if (selectedMappedItem != null)
            {
                // Return to available lists
                CsvColumns.Add(selectedMappedItem.CsvColumn);
                DatabaseColumns.Add(selectedMappedItem.DatabaseColumn);

                // Remove from mapped columns
                MappedColumns.Remove(selectedMappedItem);

                // Refresh DataGrids
                CsvColumnsGrid.ItemsSource = null;
                CsvColumnsGrid.ItemsSource = CsvColumns;

                DatabaseColumnsGrid.ItemsSource = null;
                DatabaseColumnsGrid.ItemsSource = DatabaseColumns;

                MappedColumnsGrid.ItemsSource = null;
                MappedColumnsGrid.ItemsSource = MappedColumns;
            }
            else if (selectedIgnoredItem != null)
            {
                // Return to available CSV columns
                CsvColumns.Add(selectedIgnoredItem.CsvColumn);

                // Remove from ignored columns
                IgnoredColumns.Remove(selectedIgnoredItem);

                // Refresh DataGrids
                CsvColumnsGrid.ItemsSource = null;
                CsvColumnsGrid.ItemsSource = CsvColumns;

                IgnoredColumnsGrid.ItemsSource = null;
                IgnoredColumnsGrid.ItemsSource = IgnoredColumns;
            }

            UpdateUIState();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Group mappings by CSV row (dataset)
            var groupedMappings = csvData.Select(row =>
            {
                var addressObj = new Addresses();
                var locationObj = new Locations();

                foreach (var mapping in MappedColumns)
                {
                    if (row.ContainsKey(mapping.CsvColumn))
                    {
                        var mappedValue = row[mapping.CsvColumn];

                        if (MainWindow.DatabaseAddressesColumns.Any(col => col.Name == mapping.DatabaseColumn))
                        {
                            var dataType = MainWindow.DatabaseAddressesColumns
                                .First(col => col.Name == mapping.DatabaseColumn).DataType;

                            addressObj.GetType()
                                .GetProperty(mapping.DatabaseColumn)?
                                .SetValue(addressObj, ConvertValue(mappedValue, dataType));
                        }
                        else if (MainWindow.DatabaseLocationsColumns.Any(col => col.Name == mapping.DatabaseColumn))
                        {
                            var dataType = MainWindow.DatabaseLocationsColumns
                                .First(col => col.Name == mapping.DatabaseColumn).DataType;

                            locationObj.GetType()
                                .GetProperty(mapping.DatabaseColumn)?
                                .SetValue(locationObj, ConvertValue(mappedValue, dataType));
                        }
                    }
                }

                return new Addresses()
                {
                    Vorname = addressObj.Vorname,
                    Name = addressObj.Name,
                    Firma = addressObj.Firma,
                    Strasse = addressObj.Strasse,
                    Hausnummer = addressObj.Hausnummer,
                    Locations = new Locations()
                    {
                        Postleitzahl = locationObj.Postleitzahl,
                        Ortschaft = locationObj.Ortschaft
                    }
                };
            }).ToList();

            // Assign to MappedDatabaseObjects for further processing
            MappedDatabaseObjects = groupedMappings;

            MessageBox.Show($"Successfully mapped {MappedDatabaseObjects.Count} database objects.");
            this.DialogResult = true;
            this.Close();
        }

        public object ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            try
            {
                if (targetType == typeof(int)) return int.Parse(value);
                if (targetType == typeof(long)) return long.Parse(value);
                if (targetType == typeof(decimal)) return decimal.Parse(value);
                if (targetType == typeof(double)) return double.Parse(value);
                if (targetType == typeof(bool)) return bool.Parse(value);
                if (targetType == typeof(DateTime)) return DateTime.Parse(value);
                return value; // Default: Assume string
            }
            catch
            {
                throw new InvalidCastException($"Could not convert '{value}' to {targetType.Name}.");
            }
        }
        private void UpdateUIState()
        {
            // Enable Add Mapping button only if an item is selected in both top grids
            AddMappingButton.IsEnabled = CsvColumnsGrid.SelectedItem != null && DatabaseColumnsGrid.SelectedItem != null;

            // Enable Remove Mapping button only if an item is selected in one of the bottom grids
            RemoveMappingButton.IsEnabled = MappedColumnsGrid.SelectedItem != null || IgnoredColumnsGrid.SelectedItem != null;

            // Show Save button only when all columns are mapped or ignored
            SaveButton.IsEnabled = CsvColumns.Count == 0;
        }

        // Class to represent mappings
        public class Mapping
        {
            public string CsvColumn { get; set; }
            public string DatabaseColumn { get; set; }
        }

        private void DatabaseColumnsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUIState();
        }

        private void CsvColumnsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUIState();
        }

        private void MappedColumnsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUIState();
        }

        private void IgnoredColumnsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUIState();
        }
    }
}