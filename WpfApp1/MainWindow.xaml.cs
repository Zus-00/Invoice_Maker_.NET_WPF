using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace InvoiceApp
{
    public partial class MainWindow : Window
    {
        // Constructor for the main window
        public MainWindow()
        {
            InitializeComponent();
        }

        // Event handler for opening an invoice file
        private void OpenInvoice_Click(object sender, RoutedEventArgs e)
        {
            // Create an OpenFileDialog to select an invoice file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            // Show the dialog and check if a file was selected
            if (openFileDialog.ShowDialog() == true)
            {
                // Read all lines from the selected file
                string[] lines = File.ReadAllLines(openFileDialog.FileName);

                // Update invoice details from the file
                var invoiceInfo = (InvoiceNumber: lines[0], InvoiceDate: DateTime.Parse(lines[1]), DueDate: DateTime.Parse(lines[2]));
                InvoiceNumberTextBlock.Text = invoiceInfo.InvoiceNumber;
                InvoiceDatePicker.SelectedDate = invoiceInfo.InvoiceDate;
                DueDatePicker.SelectedDate = invoiceInfo.DueDate;

                // Update company information from the file
                CompanyNameTextBlock.Text = lines[3];
                CompanyAddressLine1TextBlock.Text = lines[4];
                CompanyAddressLine2TextBlock.Text = lines[5];
                CompanyAddressLine3TextBlock.Text = lines[6] +" "+ lines[7];
                CompanyCountryTextBlock.Text = lines[8];

                // Read and update the DataGrid with invoice items
                int numberOfItems = int.Parse(lines[9]);
                var items = new List<InvoiceItem>();
                for (int i = 0; i < numberOfItems; i++)
                {
                    string description = lines[10 + i * 4];
                    int quantity = int.Parse(lines[11 + i * 4]);
                    float price = ParseStringToFloat(lines[12 + i * 4 ]);
                    float tax = ParseStringToFloat(lines[13 + i * 4]);
                    items.Add(new InvoiceItem(description, quantity, price, tax));
                }

                // Set the ItemsSource of the DataGrid to the list of items
                ItemsDataGrid.ItemsSource = items;

                // Update address and contact information from the file
                int lineIndex = 10 + numberOfItems * 4;
                Address address = new Address(
                    lines[lineIndex],
                    lines[lineIndex + 1],
                    lines[lineIndex + 2],
                    lines[lineIndex + 3]
                );
                AddressTextBlock.Text = $"{lines[lineIndex + 1]}\n{lines[lineIndex + 2]} {lines[lineIndex + 3]}\n{lines[lineIndex + 4]}";
                ContactTextBlock.Text = lines[lineIndex + 5];
                WebsiteTextBlock.Text = lines[lineIndex + 6];

                // Recalculate and update the total amount
                CalculateTotal();
            }
        }

        // Event handler for text change in the DiscountTextBox
        private void DiscountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Recalculate the total amount whenever the discount changes
            CalculateTotal();
        }

        // Event handler for exiting the application
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Close the main window
            this.Close();
        }

        // Function to calculate the total amount of the invoice
        private void CalculateTotal()
        {
            double total = 0;

            // Sum the total for each item in the DataGrid
            foreach (InvoiceItem item in ItemsDataGrid.Items)
            {
                total += item.Total;
            }

            // Apply discount if provided
            total -= ParseStringToFloat(DiscountTextBox.Text);

            // Update the TotalAmountTextBlock with the calculated total
            TotalAmountTextBlock.Text = total.ToString("F2");
        }

        public float ParseStringToFloat(string input)
        {
            // Try to parse the string as a float initially
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }

            // If parsing failed, remove commas and try again
            string modifiedInput = input.Replace(",", ".");
            if (float.TryParse(modifiedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            // If both attempts fail, throw an exception
            return 0;
        }
    }

    // Record to represent address information
    public record Address(string Line1, string Line2, string City, string Country);

    // Class to represent an invoice item
    public class InvoiceItem
    {
        // Tuple to group related information
        private (string Description, int Quantity, float Price, float Tax) itemInfo;

        // Properties to access tuple elements
        public string Description
        {
            get => itemInfo.Description;
            set => itemInfo.Description = value;
        }
        public int Quantity
        {
            get => itemInfo.Quantity;
            set => itemInfo.Quantity = value;
        }
        public float Price
        {
            get => itemInfo.Price;
            set => itemInfo.Price = value;
        }
        public float Tax
        {
            get => itemInfo.Tax;
            set => itemInfo.Tax = value;
        }

        // Computed properties
        public float TotalTax => Quantity * Price * (Tax / 100);
        public float Total => Quantity * Price * (1 + Tax / 100);

        // Constructor to initialize an invoice item
        public InvoiceItem(string description, int quantity, float price, float tax)
        {
            itemInfo = (description, quantity, price, tax);
        }
    }
}
