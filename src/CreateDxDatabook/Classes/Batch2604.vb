Imports System.IO
Imports System.Linq
Imports System.Threading
Imports DxDbClass
Imports DxDbClass.DxDB

' Uses the same PDB, SQLite and DBC classes as the interactive application.
Public Module Batch2604
    Private log As StreamWriter
    Private errors As Integer
    Public Function Run(args As String()) As Integer
        Try
            Dim options As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            Dim flags As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim i As Integer = 0
            While i < args.Length
                Select Case args(i).ToLowerInvariant()
                    Case "--batch", "--symbols", "--cell-pins", "--allow-duplicates"
                        flags.Add(args(i))
                    Case "--lmc", "--out", "--log", "--psf", "--delimiter"
                        If i + 1 >= args.Length Then Throw New ArgumentException("Missing value for " & args(i))
                        options.Add(args(i), args(i + 1))
                        i += 1
                    Case Else
                        Throw New ArgumentException("Unknown argument: " & args(i))
                End Select
                i += 1
            End While
            If Not options.ContainsKey("--lmc") OrElse Not options.ContainsKey("--out") Then
                Throw New ArgumentException("Usage: CreateDxDatabook.exe --batch --lmc library.lmc --out output [--log log.txt] [--symbols] [--cell-pins] [--allow-duplicates] [--psf file --delimiter |]")
            End If
            Dim lmc As String = Path.GetFullPath(options("--lmc"))
            Dim output As String = Path.GetFullPath(options("--out"))
            Directory.CreateDirectory(output)
            Dim logPath As String = If(options.ContainsKey("--log"), options("--log"), Path.Combine(output, "generation.log"))
            ' Validate destinations before opening the log (StreamWriter truncates it).
            If Not File.Exists(lmc) Then Throw New FileNotFoundException("LMC library not found.", lmc)
            If Not String.Equals(Path.GetExtension(lmc), ".lmc", StringComparison.OrdinalIgnoreCase) Then Throw New ArgumentException("--lmc must name a .lmc file.")
            Dim dbPath As String = Path.Combine(output, Path.GetFileNameWithoutExtension(lmc) & ".db3")
            Dim paths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {lmc, dbPath, Path.ChangeExtension(dbPath, ".dbc")}
            If options.ContainsKey("--psf") AndAlso Not paths.Add(Path.GetFullPath(options("--psf"))) Then Throw New ArgumentException("Side file collides with the library or an output.")
            If Not paths.Add(Path.GetFullPath(logPath)) Then Throw New ArgumentException("Log path collides with the library, side file or an output.")
            log = New StreamWriter(logPath, False, System.Text.Encoding.UTF8)
            log.AutoFlush = True
            errors = 0
            Write("CreateDxDatabook XENTP2604; STA=" & Thread.CurrentThread.GetApartmentState().ToString())
            Dim release As New SddVxEnv(SddVxEnv.VxAppInstType.Main, SddVxEnv.TargetComVersion)
            release.SetSddEnv(SddVxEnv.TargetComVersion)
            Write("Selected " & release.ActiveRelease.Name & "; COM=" & release.ActiveRelease.ComVersion.ToString() & "; SDD_HOME=" & release.ActiveRelease.SddHome)
            Dim parts As New Pdb(lmc, SddVxEnv.TargetComVersion)
            Observe(parts)
            Dim symbols As Boolean = flags.Contains("--symbols")
            Dim pins As Boolean = flags.Contains("--cell-pins")
            Dim duplicates As Boolean = flags.Contains("--allow-duplicates")
            parts.ReadPDB(symbols, pins)
            Dim count As Integer = parts.Partitions.Sum(Function(p) p.Parts.Count)
            Write("PDB_RESULT partitions=" & parts.Partitions.Count.ToString() & " parts=" & count.ToString())
            If count = 0 Then Throw New InvalidOperationException("PDB contains no parts; generation was stopped.")
            If options.ContainsKey("--psf") Then
                Dim side As New SideFileDB(SddVxEnv.TargetComVersion)
                Observe(side)
                Dim delimiter As String = If(options.ContainsKey("--delimiter"), options("--delimiter"), "|")
                If Not side.ReadPropsFile(options("--psf"), delimiter) Then Throw New InvalidOperationException("Cannot read property side file.")
                parts.MergePropsDb(side)
            End If
            If errors > 0 Then Throw New InvalidOperationException("Errors while reading PDB/side file: " & errors.ToString())
            Dim database As New SqliteADO(lmc, duplicates, symbols, pins, SddVxEnv.TargetComVersion)
            Observe(database)
            If Not database.Create(parts, output) OrElse errors > 0 Then Throw New InvalidOperationException("SQLite generation failed.")
            Dim dbFile As String = Path.Combine(output, Path.GetFileNameWithoutExtension(lmc) & ".db3")
            Dim dbcFile As String = Path.ChangeExtension(dbFile, ".dbc")
            Dim config As New Dbc(lmc, dbcFile, SddVxEnv.TargetComVersion, False, parts)
            Observe(config)
            config.CreateDatabaseDbcCfg(dbFile, symbols, duplicates)
            config.Save()
            If errors > 0 OrElse Not File.Exists(dbFile) OrElse Not File.Exists(dbcFile) Then Throw New InvalidOperationException("Databook generation failed.")
            Write("SUCCESS: " & dbFile)
            Write("SUCCESS: " & dbcFile)
            Return 0
        Catch ex As Exception
            Write("FAILED: " & ex.ToString())
            Return 1
        Finally
            If log IsNot Nothing Then
                log.Dispose()
                log = Nothing
            End If
        End Try
    End Function
    Private Sub Observe(source As BaseClass)
        AddHandler source.TranscriptMsg, AddressOf Transcript
        AddHandler source.SysException, AddressOf Failure
    End Sub
    Private Sub Transcript(message As String, spaces As Integer, kind As MsgType, addLog As Boolean)
        If kind = MsgType.Err Then errors += 1
        Write(New String(" "c, spaces) & message)
    End Sub
    Private Sub Failure(message As String, ex As Exception, spaces As Integer)
        errors += 1
        Write(message & ": " & ex.ToString())
    End Sub
    Private Sub Write(message As String)
        Console.WriteLine(message)
        If log IsNot Nothing Then log.WriteLine(message)
    End Sub
End Module
