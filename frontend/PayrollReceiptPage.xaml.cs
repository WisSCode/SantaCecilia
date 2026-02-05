using System;
using Microsoft.Maui.Controls;

namespace frontend;

public partial class PayrollReceiptPage : ContentPage
{
    public PayrollReceiptPage(string html)
    {
        InitializeComponent();

        var source = new HtmlWebViewSource { Html = html };
        ReceiptWebView.Source = source;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
