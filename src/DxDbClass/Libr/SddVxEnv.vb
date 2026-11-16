Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports MGCPCBReleaseEnvironmentLib

' Only this verified automation contract is supported by this build.
Public Class SddVxEnv
    Public Const TargetComVersion As Integer = 151
    Public Const TargetRelease As String = "XENTP2604"
    Public Enum VxAppInstType
        Main
        Client
    End Enum
    Public Enum SddPlatformType
        win32
        win64
    End Enum
    Private _ActiveRelease As VxRelease
    Private _Releases As New List(Of VxRelease)
    Public ReadOnly Property RequiredComversion As Integer
        Get
            Return TargetComVersion
        End Get
    End Property
    Public ReadOnly Property ActiveRelease As VxRelease
        Get
            Return _ActiveRelease
        End Get
    End Property
    Public ReadOnly Property Releases As List(Of VxRelease)
        Get
            Return _Releases
        End Get
    End Property
    Public ReadOnly Property IsValid As Boolean
        Get
            Return Environment.Is64BitProcess AndAlso _ActiveRelease IsNot Nothing AndAlso
                _ActiveRelease.ComVersion = TargetComVersion AndAlso
                _ActiveRelease.Name = TargetRelease AndAlso _ActiveRelease.Platform = SddPlatformType.win64
        End Get
    End Property
    Public Sub New(instance As VxAppInstType, requiredComVersion As Integer)
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture
        If requiredComVersion <> TargetComVersion Then Throw New InvalidOperationException("This build requires COM 151 (XENTP2604).")
        If Not Environment.Is64BitProcess Then Throw New PlatformNotSupportedException("XENTP2604 requires an x64 process.")
        Dim server As MGCPCBReleaseEnvServer = Nothing
        Try
            server = New MGCPCBReleaseEnvServer()
            Dim entries As Array = DirectCast(server.GetInstalledReleases(), Array)
            Dim requestedHome As String = Environment.GetEnvironmentVariable("CRDB2604_SDD_HOME")
            For i As Integer = entries.GetLowerBound(0) To entries.GetUpperBound(0)
                Dim version As Integer
                If Not Integer.TryParse(Convert.ToString(entries.GetValue(i, 0)), version) Then Continue For
                Dim name As String = Convert.ToString(entries.GetValue(i, 3))
                Dim platform As String = Convert.ToString(entries.GetValue(i, 2))
                Dim home As String = Convert.ToString(entries.GetValue(i, 1))
                If version <> TargetComVersion OrElse name <> TargetRelease OrElse platform <> "win64" Then Continue For
                If Not String.IsNullOrWhiteSpace(requestedHome) AndAlso Not SamePath(home, requestedHome) Then Continue For
                _Releases.Add(New VxRelease(home, name, SddPlatformType.win64, version))
            Next
            If _Releases.Count <> 1 Then Throw New InvalidOperationException("Exactly one registered XENTP2604/win64/COM151 release is required. Use CRDB2604_SDD_HOME to select its SDD_HOME. Matches: " & _Releases.Count.ToString())
            _ActiveRelease = _Releases(0)
        Finally
            If server IsNot Nothing Then Marshal.FinalReleaseComObject(server)
        End Try
    End Sub
    Private Shared Function SamePath(a As String, b As String) As Boolean
        Return String.Equals(Path.GetFullPath(a).TrimEnd("\"c), Path.GetFullPath(b).TrimEnd("\"c), StringComparison.OrdinalIgnoreCase)
    End Function
    Private Sub ValidateServer(progId As String, subKey As String, relativePath As String)
        Using classes As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64)
            Using id As RegistryKey = classes.OpenSubKey(progId & "\CLSID")
                If id Is Nothing Then Throw New InvalidOperationException("COM class is not registered: " & progId)
                Using location As RegistryKey = classes.OpenSubKey("CLSID\" & CStr(id.GetValue("")) & "\" & subKey)
                    If location Is Nothing Then Throw New InvalidOperationException("COM server is missing: " & progId)
                    Dim actual As String = CStr(location.GetValue("")).Trim().Trim(""""c)
                    Dim expected As String = Path.Combine(_ActiveRelease.SddHome, relativePath)
                    If Not SamePath(actual, expected) OrElse Not File.Exists(expected) Then
                        Throw New InvalidOperationException("Incorrect COM registration for " & progId & ": " & actual & "; expected " & expected)
                    End If
                End Using
            End Using
        End Using
    End Sub
    Public Function SetSddEnv(comVersion As Integer) As Boolean
        If comVersion <> TargetComVersion OrElse Not IsValid Then Throw New InvalidOperationException("Invalid XENTP2604 release selection.")
        Dim server As MGCPCBReleaseEnvServer = Nothing
        Try
            server = New MGCPCBReleaseEnvServer()
            server.SetEnvironment(_ActiveRelease.SddHome)
            If Convert.ToInt32(server.ProgIDVersion) <> TargetComVersion OrElse
                server.sddVersion <> TargetRelease OrElse server.sddPlatform <> "win64" OrElse
                Not SamePath(server.sddHome, _ActiveRelease.SddHome) Then
                Throw New InvalidOperationException("Release environment server did not select XENTP2604/win64/COM151.")
            End If
            ValidateServer("MGCPCBLibraries.PartsEditorDlg.151", "InprocServer32", "common\win64\lib\PDBEditor.dll")
            ValidateServer("LibraryManager.Application.151", "LocalServer32", "lm\win64\bin\LibraryManager.exe")
            Return True
        Finally
            If server IsNot Nothing Then Marshal.FinalReleaseComObject(server)
        End Try
    End Function
    Public Sub ShowNoReleaseMsgBox()
        MsgBox("XENTP2604 / win64 / COM151 is required. Verify the installation and COM registration.", MsgBoxStyle.Exclamation)
    End Sub
End Class

Public Class VxRelease

#Region "Declarations"
    Protected _Platform As SddVxEnv.SddPlatformType
    Protected _Name As String
    Protected _Number As Double
    Protected _ComVersion As Integer
    Protected _RootPath As String
    Protected _SddHome As String
#End Region

#Region "Public Methods"
    Public Sub New(SddHome As String, SddVersion As String, SddPlatform As SddVxEnv.SddPlatformType, ComVersion As Integer)
        Me._Platform = SddPlatform
        Me._SddHome = SddHome
        Me._ComVersion = ComVersion
        Me._Name = SddVersion
        Me._Number = -1
        Me._RootPath = Path.GetDirectoryName(SddHome)

        Me._Number = Me.SetReleaseNumber()
    End Sub

    Public Function GetMgcToolExePath(ExeName As String) As String
        Dim ToolPath As String
        If String.IsNullOrEmpty(Path.GetExtension(ExeName)) Then
            ExeName = ExeName + ".exe"
        End If
        ToolPath = Me.SddHome + "/common/" + Me.Platform.ToString + "/bin/" + ExeName
        If File.Exists(ToolPath) Then Return ToolPath
        Return String.Empty
    End Function
#End Region

#Region "Properties"
    Public ReadOnly Property Platform As SddVxEnv.SddPlatformType
        Get
            Return Me._Platform
        End Get
    End Property

    Public ReadOnly Property Name As String
        Get
            Return Me._Name
        End Get
    End Property

    Public ReadOnly Property Number As Double
        Get
            Return Me._Number
        End Get
    End Property

    Public ReadOnly Property ComVersion As Integer
        Get
            Return Me._ComVersion
        End Get
    End Property

    Public ReadOnly Property RootPath As String
        Get
            Return Me._RootPath
        End Get
    End Property

    Public ReadOnly Property SddHome As String
        Get
            Return Me._SddHome
        End Get
    End Property

    Public ReadOnly Property LmUtil As String
        Get
            Dim Exe As String
            If Me.Platform = SddVxEnv.SddPlatformType.win32 Then
                Exe = Me._SddHome + "\common\" + Me._Platform.ToString + "\bin\lmutil.exe"
            Else
                Exe = Me._SddHome + "\common\" + Me._Platform.ToString + "\bin\lmutil64.exe"
            End If
            If File.Exists(Exe) Then
                Return Exe
            End If
            Return "<File Not Found!>"
        End Get
    End Property

    Public ReadOnly Property LmTools As String
        Get
            Dim Exe As String 'C:\MentorGraphics.x64\EEVX.2.6\SDD_HOME\common\win64\bin
            If Me.Platform = SddVxEnv.SddPlatformType.win32 Then
                Exe = Me._SddHome + "\common\" + Me._Platform.ToString + "\bin\lmtools.exe"
            Else
                Exe = Me._SddHome + "\common\" + Me._Platform.ToString + "\bin\lmtools64.exe"
            End If
            If File.Exists(Exe) Then
                Return Exe
            End If
            Return "<File Not Found!>"
        End Get
    End Property

    Public ReadOnly Property MgCmd As String
        Get
            Dim Exe As String = Me._SddHome + "\common\" + Me._Platform.ToString + "\bin\mgcmd.exe"
            If My.Computer.FileSystem.FileExists(Exe) Then
                Return Exe
            End If
            Return "<File Not Found!>"
        End Get
    End Property

    Public ReadOnly Property MgLaunch As String
        Get
            Dim Exe As String = Me._SddHome + "\common\" + Me._Platform.ToString + "\bin\mglaunch.exe"
            If My.Computer.FileSystem.FileExists(Exe) Then
                Return Exe
            End If
            Return "<File Not Found!>"
        End Get
    End Property

    Public ReadOnly Property MgInvoke As String
        Get
            Dim Exe As String = Me._SddHome + "\common\" + Me._Platform.ToString + "\bin\mginvoke.exe"
            If My.Computer.FileSystem.FileExists(Exe) Then
                Return Exe
            End If
            Return "<File Not Found!>"
        End Get
    End Property

    Public ReadOnly Property MgcScript As String
        Get
            Dim Exe As String = Me._SddHome + "\common\" + Me._Platform.ToString + "\bin\mgcscript.exe"
            If My.Computer.FileSystem.FileExists(Exe) Then
                Return Exe
            End If
            Return "<File Not Found!>"
        End Get
    End Property

    Public ReadOnly Property ReleaseSwitcher As String
        Get
            Dim Exe As String = Me.SddHome + "\common\" + Me._Platform.ToString + "\_bin\ReleaseSwitcher.exe"
            If My.Computer.FileSystem.FileExists(Exe) Then
                Return Exe
            End If
            Return "<File Not Found!>"
        End Get
    End Property

    Public ReadOnly Property SutHome As String
        Get
            Dim SutDir As String = Me.SddHome + "\EDM-Server\Utilities"
            If My.Computer.FileSystem.DirectoryExists(SutDir) Then
                Return SutDir
            End If
            Return "<Directory Not Found!>"
        End Get
    End Property
#End Region

#Region "Private Methods"
    Private Function SetReleaseNumber() As Integer
        'Dim Pat As String = "EEVX\.([0-9]{1})\.([0-9]+)"
        If Me.Name = SddVxEnv.TargetRelease Then Return 2604
        Dim Pat As String = "EEVX\.([0-9]{1})\.([0-9]+)\.?([0-9]*)"
        Dim RegM As Match, RegEx As New Regex(Pat)
        Dim Major As Integer = -1, Minor As Integer = -1, Ext As Integer = 0

        RegM = RegEx.Match(Me.Name)
        If RegM.Success Then
            Major = Integer.Parse(RegM.Groups.Item(1).Value) * 100
            Minor = Integer.Parse(RegM.Groups.Item(2).Value)
            If Not String.IsNullOrEmpty(RegM.Groups.Item(3).Value) Then
                Ext = Integer.Parse(RegM.Groups.Item(3).Value)
            End If
            Return Major + Minor + Ext
        End If

        Return -1
    End Function
#End Region

End Class
