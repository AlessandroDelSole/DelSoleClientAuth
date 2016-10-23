Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class AuthService

    ''' <summary>
    ''' Return the base address of the Web API service (e.g. http://servername/servicename)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BaseUri As String

    Private _isAuthenticated As Boolean

    ''' <summary>
    ''' Return if the user has been successfully authenticated after invoking <seealso cref="LoginAsync(String, String)"/> 
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsAuthenticated As Boolean
        Get
            Return _isAuthenticated
        End Get
    End Property

    Private _token As TokenModel

    ''' <summary>
    ''' Return full information from the token response. This includes the access token, expiration time, expiration date (if any), issued date (if any)
    ''' </summary>
    ''' <returns><seealso cref="TokenModel"/></returns>
    Public ReadOnly Property Token As TokenModel
        Get
            Return _token
        End Get
    End Property

    ''' <summary>
    ''' Creates an instance of the AuthService class
    ''' </summary>
    ''' <param name="baseUri">The Web API service base address</param>
    Public Sub New(baseUri As String)
        Me.BaseUri = baseUri
        Me._isAuthenticated = False
    End Sub

    ''' <summary>
    ''' Authenticate the user with username and password against the Web API service and returns a <seealso cref="TokenModel"/> instance
    ''' </summary>
    ''' <param name="userName"></param>
    ''' <param name="password"></param>
    ''' <returns><seealso cref="TokenModel"/></returns>
    Protected Async Function GetTokenAsync(userName As String, password As String) As Task(Of TokenModel)
        Using client = New HttpClient()
            'setup client
            client.BaseAddress = New Uri(Me.BaseUri)
            client.DefaultRequestHeaders.Accept.Clear()
            client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))

            Dim loginInfo As New List(Of KeyValuePair(Of String, String)) From {New KeyValuePair(Of String, String)("grant_type", "password"), New KeyValuePair(Of String, String)("username", userName), New KeyValuePair(Of String, String)("password", password)}

            'setup login data
            Dim formContent = New FormUrlEncodedContent(loginInfo.AsEnumerable())

            'send request
            Dim responseMessage As HttpResponseMessage = Await client.PostAsync("token", formContent)

            'get access token from response body
            Dim responseJson = Await responseMessage.Content.ReadAsStringAsync()
            Dim response = JObject.Parse(responseJson)

            Dim model As New TokenModel
            model.AccessToken = response.GetValue("access_token").ToString()

            Try
                Dim expirationTicks As Integer
                Dim expiresIn = Integer.TryParse(response.GetValue("expires_in").ToString(), expirationTicks)
                If expiresIn = True Then Token.ExpiresIn = TimeSpan.FromTicks(expirationTicks)

                Dim expirationDate As Date
                Dim expiresAt = Date.TryParse(response.GetValue(".expires").ToString(), expirationDate)
                If expiresAt = True Then model.ExpiresAt = expirationDate

                Dim issuanceDate As Date
                Dim issuedAt = Date.TryParse(response.GetValue(".issued").ToString(), issuanceDate)
                If issuedAt = True Then model.IssuedAt = issuanceDate
            Catch ex As Exception

            End Try

            Return model
        End Using
    End Function

    ''' <summary>
    ''' Authenticate the user with username and password against the Web API service and returns a <seealso cref="TokenModel"/> instance
    ''' </summary>
    ''' <param name="userName"></param>
    ''' <param name="password"></param>
    ''' <returns></returns>
    Public Async Function LoginAsync(userName As String, password As String) As Task(Of TokenModel)
        Dim token = Await GetTokenAsync(userName, password)

        If Not String.IsNullOrEmpty(token.AccessToken) Then Me._isAuthenticated = True Else Me._isAuthenticated = False
        Me._token = token
        Return token
    End Function

    ''' <summary>
    ''' Serialize the specified object sending a POST request to the Web API controller
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objectInstance">The object to be serialized</param>
    ''' <param name="controllerUri">The relative URI of the Web API controller (e.g. api/Customers)</param>
    ''' <returns><seealso cref="HttpStatusCode"/></returns>
    Public Async Function PostAsync(Of T)(objectInstance As T, controllerUri As String) As Task(Of HttpStatusCode)
        If Me.Token.AccessToken Is Nothing Then
            Throw New UnauthorizedException("The access token has not been requested or has been denied")
        End If

        Using client As New HttpClient
            client.BaseAddress = New Uri(Me.BaseUri)
            client.DefaultRequestHeaders.Accept.Clear()
            client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
            client.DefaultRequestHeaders.Add("Authorization", Convert.ToString("Bearer ") & Token.AccessToken)

            Dim content As New StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(objectInstance), Encoding.UTF8, "application/json")

            Dim response As HttpResponseMessage = Await client.PostAsync(controllerUri, content)

            Return response.StatusCode
        End Using
    End Function

    ''' <summary>
    ''' Deserialize the specified type sending an HTTP GET request to the Web API controller
    ''' </summary>
    ''' <typeparam name="T">The destination type of deserialization</typeparam>
    ''' <param name="requestUri">The Web API controller relative URI (e.g.) api/Customers</param>
    ''' <returns></returns>
    Public Async Function GetAsync(Of T)(requestUri As String) As Task(Of T)
        If Me.Token.AccessToken Is Nothing Then
            Throw New UnauthorizedException("The access token has not been requested or has been denied")
        End If

        Using client As New HttpClient()
            'setup client
            client.BaseAddress = New Uri(Me.BaseUri)
            client.DefaultRequestHeaders.Accept.Clear()
            client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
            client.DefaultRequestHeaders.Add("Authorization", Convert.ToString("Bearer ") & Token.AccessToken)

            'make request
            Dim response As HttpResponseMessage = Await client.GetAsync(requestUri)
            Dim responseString = Await response.Content.ReadAsStringAsync()

            Return JsonConvert.DeserializeObject(Of T)(responseString)
        End Using
    End Function

    ''' <summary>
    ''' Delete an object sending an HTTP DELETE request to the Web API service
    ''' </summary>
    ''' <param name="requestUri">The Web API controller relative URI (e.g.) api/Customers</param>
    ''' <returns></returns>
    Public Async Function DeleteAsync(requestUri As String) As Task(Of HttpStatusCode)
        If Me.Token.AccessToken Is Nothing Then
            Throw New UnauthorizedException("The access token has not been requested or has been denied")
        End If

        Using client As New HttpClient
            client.BaseAddress = New Uri(Me.BaseUri)
            client.DefaultRequestHeaders.Accept.Clear()
            client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
            client.DefaultRequestHeaders.Add("Authorization", Convert.ToString("Bearer ") & Token.AccessToken)

            Dim response As HttpResponseMessage = Await client.DeleteAsync(requestUri)

            Return response.StatusCode
        End Using
    End Function

    ''' <summary>
    ''' Update the specified object sending a PUT request to the Web API controller
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objectInstance">The object to be serialized</param>
    ''' <param name="requestUri">The relative URI of the Web API controller (e.g. api/Customers)</param>
    ''' <returns><seealso cref="HttpStatusCode"/></returns>
    Public Async Function PutAsync(Of T)(objectInstance As T, requestUri As String) As Task(Of HttpStatusCode)
        If Me.Token.AccessToken Is Nothing Then
            Throw New UnauthorizedException("The access token has not been requested or has been denied")
        End If

        Using client As New HttpClient
            client.BaseAddress = New Uri(Me.BaseUri)
            client.DefaultRequestHeaders.Accept.Clear()
            client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
            client.DefaultRequestHeaders.Add("Authorization", Convert.ToString("Bearer ") & Token.AccessToken)

            Dim content As New StringContent(JsonConvert.SerializeObject(objectInstance), Encoding.UTF8, "application/json")

            Dim response As HttpResponseMessage = Await client.PutAsync(requestUri, content)
            Return response.StatusCode
        End Using
    End Function
End Class
