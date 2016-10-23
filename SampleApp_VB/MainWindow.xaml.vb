Imports DelSole.ClientAuth
Class MainWindow

    Private service As AuthService

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.service = New AuthService("https://localhost:28888") 'Replace with your Web API based address
    End Sub

    Private Async Sub button_Click(sender As Object, e As RoutedEventArgs) Handles button.Click
        Dim token = Await service.LoginAsync("admin", "admin")

        If service.IsAuthenticated Then
            MessageBox.Show($"{token.AccessToken} issued at {token.IssuedAt?.ToLongDateString()}, expires on {token.ExpiresAt.ToString()}")
        End If

    End Sub
End Class
