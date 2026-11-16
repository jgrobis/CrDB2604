Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports MGCPCBPartsEditor
Imports System.Globalization
Imports Microsoft.VisualBasic.FileIO
Imports System.Runtime.Serialization.Formatters.Binary

Namespace DxDB
    Public Class Pdb
        Inherits BaseClass

#Region "Members"
        Private ComVersion As Integer
        Private _LmcFile As String
        Private _Partitions As List(Of PdbPrtn)
#End Region

#Region "Properties"
        Public ReadOnly Property LmcFile() As String
            Get
                Return Me._LmcFile
            End Get
        End Property

        Public ReadOnly Property Partitions() As List(Of PdbPrtn)
            Get
                Return Me._Partitions
            End Get
        End Property

        Public ReadOnly Property Partition(ByVal p As Integer) As PdbPrtn
            Get
                Return Me._Partitions.Item(p)
            End Get
        End Property
#End Region

#Region "Constructor Methods"
        Public Sub New(LmcFile As String, nComVersion As Integer)
            MyBase.New(nComVersion)
            Me.ComVersion = nComVersion
            Me._LmcFile = LmcFile
            Me._Partitions = New List(Of PdbPrtn)
        End Sub

        Public Sub New(CurPdb As Pdb, nComVersion As Integer)
            MyBase.New(nComVersion)
            Me.ComVersion = nComVersion
            Me._LmcFile = CurPdb.LmcFile
            Me._Partitions = New List(Of PdbPrtn)
            For Each Prtn As PdbPrtn In CurPdb.Partitions
                Me._Partitions.Add(Prtn)
            Next
        End Sub
#End Region

#Region "Public Methods"
        Public Sub ReadPDB(UseSymbolTable As Boolean, UseCellPins As Boolean)
            If Thread.CurrentThread.GetApartmentState() <> ApartmentState.STA Then
                Throw New InvalidOperationException("PDB automation must run on an STA thread.")
            End If
            If Not File.Exists(Me.LmcFile) Then Throw New FileNotFoundException("LMC library not found.", Me.LmcFile)
            Dim release As New SddVxEnv(SddVxEnv.VxAppInstType.Main, Me.ComVersion)
            release.SetSddEnv(Me.ComVersion)
            Me.DxDbTranscriptMsg("Release: " & release.ActiveRelease.Name & " / win64 / COM " & Me.ComVersion.ToString())
            Me.DxDbTranscriptMsg("SDD_HOME: " & release.ActiveRelease.SddHome)
            Dim editor As PartsEditorDlg = Nothing
            Dim database As PartsDB = Nothing
            Me._Partitions.Clear()
            Try
                editor = CType(CreateObject("MGCPCBLibraries.PartsEditorDlg." & Me.ComVersion.ToString(CultureInfo.InvariantCulture)), PartsEditorDlg)
                Me.DxDbTranscriptMsg("Opening LMC Parts Database ...", 5)
                database = editor.OpenDatabaseEx(Me.LmcFile, True)
                If database Is Nothing Then Throw New InvalidOperationException("OpenDatabaseEx returned no PartsDB.")
                Me.DxDbTranscriptMsg("LMC Parts Database opened successfully.", 5)
                Me.DxDbProgBarAction(database.Partitions.Count, ProgbarMode.Init)
                For Each partition As Partition In database.Partitions
                    Me.DxDbProgBarAction(0, ProgbarMode.Incr)
                    Dim data As New PdbPrtn(partition.Name, Me.ComVersion)
                    Me.AddPrtnEventHandler(data)
                    Try
                        data.ReadPdbPrtn(partition, Path.GetDirectoryName(Me.LmcFile), editor.Gui.Unit, UseSymbolTable, UseCellPins)
                        Me._Partitions.Add(data)
                    Finally
                        Me.RemPrtnEventHandler(data)
                        Marshal.ReleaseComObject(partition)
                    End Try
                Next
                Me.DxDbTranscriptMsg("Reading LMC Parts Database done", 5)
            Catch ex As Exception
                Me._Partitions.Clear()
                Throw New InvalidOperationException("Cannot read XENTP2604 PDB: " & Me.LmcFile, ex)
            Finally
                If database IsNot Nothing Then Marshal.FinalReleaseComObject(database)
                If editor IsNot Nothing Then
                    Try
                        editor.Quit()
                    Finally
                        Marshal.FinalReleaseComObject(editor)
                    End Try
                End If
                Me.DxDbProgBarAction(0, ProgbarMode.Close)
            End Try
        End Sub

        Public Sub MergePropsDb(ByRef AsciiPropsDb As SideFileDB)
            Dim i As Integer, Prtn As PdbPrtn

            If AsciiPropsDb.Parts.Count = 0 Then Exit Sub
            Me.DxDbTranscriptMsg("Merging Properties to DxDatabook", 5)

            '--------------------------------------
            ' create empty table for each partition
            Me.DxDbProgBarAction(Me.Partitions.Count, ProgbarMode.Init)
            For i = 0 To Me.Partitions.Count - 1
                ' set tablename to partition name
                Prtn = CType(Me.Partition(i), PdbPrtn)
                Me.AddPrtnEventHandler(Prtn)

                Me.DxDbTranscriptMsg("... processing Partition '" + Path.GetFileNameWithoutExtension(Prtn.Name) + "'", 10)
                Prtn.AddSideFileProps2Parts(AsciiPropsDb)
                Me.DxDbProgBarAction(0, ProgbarMode.Incr)

                Me.RemPrtnEventHandler(Prtn)
            Next

            If AsciiPropsDb.Parts.Count > 0 Then
                Me.DxDbAdd2Logfile("")
                Me.DxDbAdd2Logfile("Unmatched Sidefile Entries:", 10)
                For Each Prt As PdbPart In AsciiPropsDb.Parts
                    Me.DxDbAdd2Logfile(Prt.PartNo, 15)
                Next
            End If

            Me.DxDbProgBarAction(0, ProgbarMode.Close)
            Me.DxDbTranscriptMsg("Merging Properties to DxDatabook done", 5)
            Me.DxDbTranscriptMsg("")
        End Sub
#End Region

#Region "Add EventHandler Methods"
        Protected Sub AddPrtnEventHandler(ByRef PdbPartition As PdbPrtn)
            AddHandler PdbPartition.StatusbarMsg, AddressOf Me.DxDbStatusbarMsg
            AddHandler PdbPartition.ProgBarAction, AddressOf Me.DxDbProgBarAction
            AddHandler PdbPartition.SysException, AddressOf Me.DxDbSysException
            AddHandler PdbPartition.Add2Logfile, AddressOf Me.DxDbAdd2Logfile
            AddHandler PdbPartition.TranscriptMsg, AddressOf Me.DxDbTranscriptMsg
        End Sub

        Protected Sub RemPrtnEventHandler(ByRef PdbPartition As PdbPrtn)
            RemoveHandler PdbPartition.StatusbarMsg, AddressOf Me.DxDbStatusbarMsg
            RemoveHandler PdbPartition.ProgBarAction, AddressOf Me.DxDbProgBarAction
            RemoveHandler PdbPartition.SysException, AddressOf Me.DxDbSysException
            RemoveHandler PdbPartition.Add2Logfile, AddressOf Me.DxDbAdd2Logfile
            RemoveHandler PdbPartition.TranscriptMsg, AddressOf Me.DxDbTranscriptMsg
        End Sub
#End Region

    End Class
End Namespace