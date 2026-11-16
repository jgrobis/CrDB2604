Imports System.IO
Imports System.Xml
Imports System.Threading
Imports System.Globalization
Imports Microsoft.VisualBasic.FileIO
Imports DxDbClass.DxDB

Namespace DxDB
    Public Class Dbc
        Inherits XmlFunc

#Region "Declarations, Properties & Variables"
        Private ComVersion As Integer
        Private _DbcFile As String
        Private _LmcFile As String

        Public Overrides ReadOnly Property DbcFile() As String
            Get
                Return Me._DbcFile
            End Get
        End Property

        Public ReadOnly Property LmcFile() As String
            Get
                Return Me._LmcFile
            End Get
        End Property
#End Region

#Region "Constructor Methods"
        Public Sub New(nLmcFile As String, nDbcFile As String, nComVersion As Integer, UpdateCPD As Boolean, PartsDb As Pdb)
            MyBase.New(nComVersion, UpdateCPD)

            Me.ComVersion = nComVersion
            MyBase.UseOdbcAlias = False   ' Base Class Member
            MyBase.AllowDuplNos = False   ' Base Class Member
            MyBase.UseSymTable = False    ' Base Class Member

            Me.DbFile = ""
            Me.LmcPdb = Nothing

            Me._LmcFile = nLmcFile
            Me._DbcFile = nDbcFile

            ' set PartsDb if Not nothing
            If IsNothing(PartsDb) Then
                Me.LmcPdb = New Pdb(Me.LmcFile, nComVersion)
            Else
                Me.LmcPdb = New Pdb(PartsDb, nComVersion)
            End If
        End Sub
#End Region

#Region "Public Methods"
        Public Function LoadDbcConfig() As Boolean
            If File.Exists(DbcFile) Then
                Try
                    Me.DbcDoc.Load(Me.DbcFile)
                Catch ex As Exception
                    Return False
                End Try
                Return True
            End If
            Return False
        End Function

        Public Sub CreateDatabaseDbcCfg(nMdbFile As String, UseSymbolTable As Boolean, Optional AllowDuplPartNos As Boolean = False, Optional UseOdbcAlias As Boolean = False, Optional OdbcALias As String = "")
            Dim DbcLibraries As New List(Of XmlElement)
            Dim CCfgLibNode As XmlNode = Nothing

            MyBase.AllowDuplNos = AllowDuplPartNos
            MyBase.UseSymTable = UseSymbolTable

            Me.DbFile = nMdbFile
            If UseOdbcAlias And Not OdbcALias = String.Empty Then
                MyBase.UseOdbcAlias = UseOdbcAlias
                Me.DbFile = OdbcALias
            End If

            ' create template dbc
            If Not Me.CreateEmptyDbcCfg() Then Throw New InvalidDataException("Cannot create the DBC template.")

            '--------------------------------------
            ' get library config node <CConfigLib>
            '--------------------------------------
            If Not Me.GetNode(elemCConfigLib, CCfgLibNode) OrElse CCfgLibNode Is Nothing Then Throw New InvalidDataException("Missing CConfigLib in DBC.")

            '-----------------------------------
            ' create new XML library partitions
            '-----------------------------------
            DbcLibraries = Me.CreateLibPartitions(Me.LmcPdb.Partitions)

            '----------------------------------------------------
            ' add updated elements to <CConfigLibEntry/CObArray>
            '----------------------------------------------------
            Me.AddCObArrayChildNodes(CCfgLibNode, DbcLibraries)

            '--------------------------------------------------------------
            ' update library count <CObArray CXMLTypedPtrArraySize="(n)"\>
            '--------------------------------------------------------------
            Me.UpdateCObArraySize(CCfgLibNode)
        End Sub

        '------------------------------------------------------------
        ' Me.UpdateDbcConfig()
        '	-> Me.UpdateLibPartitions()
        '	<CConfigLib>/<CObArray>/<CConfigLibEntry>
        '------------------------------------------------------------
        Public Sub UpdateDatabaseDbcCfg(nDbFile As String, UseSymbolTable As Boolean, AllowDuplPartNos As Boolean, UseOdbcAlias As Boolean, OdbcALias As String)
            Dim DbcLibraries As New List(Of XmlElement)
            Dim CCfgLibNode As XmlNode = Nothing

            MyBase.AllowDuplNos = AllowDuplPartNos
            MyBase.UseSymTable = UseSymbolTable

            Me.DbFile = nDbFile
            If UseOdbcAlias And Not OdbcALias = String.Empty Then
                MyBase.UseOdbcAlias = UseOdbcAlias
                MyBase.DbFile = OdbcALias
            End If

            ' check DBC file
            If My.Computer.FileSystem.FileExists(Me._DbcFile) Then
                Me._DbcFile = DbcFile
                Me.DbcDoc.Load(Me.DbcFile)
                Me.DeIndexElements()
            Else
                ' create template dbc
                If Not Me.CreateEmptyDbcCfg() Then Throw New InvalidDataException("Cannot create the DBC template.")
            End If

            '--------------------------------------
            ' get library config node <CConfigLib>
            ' <CConfigLibEntry6 LibraryName="Analog" JoinTable="false" Locked="0" SymbolExpression="" JoinType="0" HorizJoinType="0">
            '--------------------------------------
            If Not Me.GetNode(elemCConfigLib, CCfgLibNode) OrElse CCfgLibNode Is Nothing Then Throw New InvalidDataException("Missing CConfigLib in DBC.")

            '----------------------------------------------------------------
            ' get/delete existing library entries <CConfigLibEntry(n) ....\>
            ' <CObArray8 CXMLTypedPtrArraySize="8">
            '   <CConfigAttEntry9 FieldName="Part Number" AttName="Part Number" DefaultValue="" ExcludeWhenAnnotating="0" ExcludeWhenLoading="0" m_bNameVisible="0" m_bValueVisible="0" AttType="4" ValueType="1" MagType="7" UnitsString="" ShowUnitsForIEC62="0" AddAsOat="0" Required="0" VerificationKey="0" ValidMag="2047" ValidMagDir="63" FutureUseBool1="0" FutureUseBool2="0" FutureUseStr1="" FutureUseStr2="" MatchCondition="--" AttrUnplace="5" SerialKey="0"/>
            '----------------------------------------------------------------
            DbcLibraries = Me.RemoveCObArrayChildNodes(CCfgLibNode)

            '----------------------------------
            ' updating existing XML partitions
            '----------------------------------
            DbcLibraries = Me.UpdateLibPartitions(DbcLibraries, Me.LmcPdb.Partitions)

            '----------------------------------------------------
            ' add updated elements to <CConfigLibEntry/CObArray>
            '----------------------------------------------------
            Me.AddCObArrayChildNodes(CCfgLibNode, DbcLibraries)

            '--------------------------------------------------------------
            ' update library count <CObArray CXMLTypedPtrArraySize="(n)"\>
            ' <CObArray8 CXMLTypedPtrArraySize="8">
            '--------------------------------------------------------------
            Me.UpdateCObArraySize(CCfgLibNode)
        End Sub

        Public Sub Save()
            Me.ReIndexElements()
            ' Keep the previous DBC intact until the complete UTF-8 XML is on disk.
            Dim destination As String = Path.GetFullPath(Me.DbcFile)
            Dim pending As String = Path.Combine(Path.GetDirectoryName(destination), Path.GetRandomFileName())
            Try
                Dim settings As New XmlWriterSettings With {.Indent = True, .Encoding = New System.Text.UTF8Encoding(False)}
                Using writer As XmlWriter = XmlWriter.Create(pending, settings)
                    Me.DbcDoc.Save(writer)
                End Using
                If File.Exists(destination) Then
                    File.Replace(pending, destination, Nothing)
                Else
                    File.Move(pending, destination)
                End If
            Finally
                If File.Exists(pending) Then File.Delete(pending)
            End Try
        End Sub
#End Region



    End Class
End Namespace

