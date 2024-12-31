# Address Manager

## Project Overview

The **Address Manager** is a Windows Presentation Foundation (WPF) application that enables users to import, map, validate, and save address data to a database. It also ensures efficient handling of address-to-location relationships by reusing or creating distinct location entries.

## Features

- **CSV Import:**
    
    - Users can load CSV files containing address data.
        
    - Automatically maps CSV columns to database columns or allows manual mapping via a mapping window.
        
- **Validation:**
    
    - Highlights invalid rows, such as those missing mandatory fields (e.g., `Hausnummer`).
- **Location Handling:**
    
    - Reuses existing `Locations` from the database based on `Postleitzahl` and `Ortschaft`.
        
    - Creates new `Locations` only when necessary.
        
- **Database Save:**
    
    - Saves addresses and ensures linked locations are stored correctly.
        
    - Efficiently handles relationships by clearing redundant references and setting `LocationID`.
        

## Technologies Used

- **C#**: Core programming language.
    
- **WPF (Windows Presentation Foundation)**: UI development.
    
- **Entity Framework**: Database interactions.
    
- **CsvHelper**: Parsing and processing CSV files.
    
- **SQL Server**: Backend database.
    

## Setup Instructions

### Prerequisites

- Visual Studio (latest version with .NET desktop development workload).
    
- SQL Server.
    
- .NET Framework or .NET Core.
    

### Steps to Run the Project

1.  **Clone the Repository:**
    
    ```
    git clone <repository-url>
    ```
    
2.  **Open the Solution:**
    
    - Open the `AddressManager.sln` file in Visual Studio.
3.  **Set Up the Database:**
    
    - Ensure that your SQL Server instance is running.
        
    - Update the `ConnectionString` in `App.config` or `appsettings.json` to match your SQL Server settings.
        
    - Run the Entity Framework migrations to set up the database:
        
        ```
        Update-Database
        ```
        
4.  **Run the Application:**
    
    - Press `F5` in Visual Studio to build and run the project.

## How to Use

### 1\. Import CSV

- Click the **Browse** button to select a CSV file.
    
- The application will load the columns from the CSV.
    

### 2\. Map Columns

- Use the **Change Mapping** button to validate and map CSV columns to database columns.
    
- Two `DataGrid` controls allow you to select CSV and database columns for mapping.
    
- Mapped and ignored columns appear in separate grids.
    

### 3\. Validate Data

- Invalid rows (e.g., missing `Hausnummer`) are highlighted in red.
    
- Fix these rows before saving.
    

### 4\. Save to Database

- Click **Save to DB** to save the addresses.
    
- Existing locations are reused, and new locations are created if required.
    

## Project Structure

```
AddressManager
├── Models
│   ├── Addresses.cs
│   ├── Locations.cs
├── Views
│   ├── MainWindow.xaml
│   ├── ValidateColumns.xaml
├── ViewModels
│   └── MainWindowViewModel.cs
├── Helpers
│   └── CsvParser.cs
├── Database
│   └── AddressManagerEntities.edmx
└── README.md
```

## Key Classes and Methods

### `MainWindow.xaml.cs`

- `ButtonBrowse_Click`: Handles the CSV file import.
    
- `ButtonSaveToDb_Click`: Saves the mapped data to the database.
    
- `EnsureDistinctLocations`: Ensures location uniqueness in the database.
    
- `SaveToDatabase`: Saves addresses and links them to the appropriate locations.
    

### `ValidateColumns.xaml.cs`

- **Purpose:** Allows users to map CSV columns to database fields manually.
    
- **Key Methods:**
    
    - `SaveButton_Click`: Processes mappings and sends them back to the main view.

## Future Enhancements

- Add support for detecting and handling duplicate addresses.
    
- Implement advanced validation rules for CSV data.
    
- Allow dynamic addition of new database fields via the UI.
    
- Provide export functionality to save database records back to a CSV.
    

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please follow these steps:

1.  Fork the repository.
    
2.  Create a new branch (`git checkout -b feature/my-feature`).
    
3.  Commit your changes (`git commit -m 'Add some feature'`).
    
4.  Push to the branch (`git push origin feature/my-feature`).
    
5.  Open a pull request.
    

## Contact

- **Author:** Rehan Ghani
    
- **Email:** rehanghani@sunrise.ch
