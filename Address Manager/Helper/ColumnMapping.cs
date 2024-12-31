using System.Collections.Generic;
using System.ComponentModel;

public partial class ColumnMapping : INotifyPropertyChanged
{
    private string _databaseColumn;

    public string CsvColumn { get; set; }

    public string DatabaseColumn
    {
        get => _databaseColumn;
        set
        {
            _databaseColumn = value;
            OnPropertyChanged(nameof(DatabaseColumn));
        }
    }

    public bool Ignore { get; set; }

    public List<string> AvailableDatabaseColumns { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}