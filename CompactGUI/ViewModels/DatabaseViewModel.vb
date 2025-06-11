﻿Imports System.Collections.ObjectModel
Imports System.ComponentModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Public Class DatabaseViewModel : Inherits ObservableObject

    Public Property DatabaseResults As ObservableCollection(Of DatabaseCompressionResult)
    Public ReadOnly Property FilteredResults As ICollectionView

    Public ReadOnly Property DatabaseGamesCount As Integer
        Get
            Return DatabaseResults.Count
        End Get
    End Property

    Public ReadOnly Property DatabaseSubmissionsCount As Integer
        Get
            Return DatabaseResults.Sum(Function(result) _
            (If(result.Result_X4K?.TotalResults, 0)) +
            (If(result.Result_X8K?.TotalResults, 0)) +
            (If(result.Result_X16K?.TotalResults, 0)) +
            (If(result.Result_LZX?.TotalResults, 0))
            )
        End Get
    End Property


    Public ReadOnly Property LastUpdatedDatabase As DateTime
        Get
            Return SettingsHandler.AppSettings.ResultsDBLastUpdated
        End Get
    End Property


    Private _searchText As String
    Public Property SearchText As String
        Get
            Return _searchText
        End Get
        Set(value As String)
            SetProperty(_searchText, value)
            FilteredResults.Refresh()
        End Set
    End Property

    Public Sub New()
        DatabaseResults = New ObservableCollection(Of DatabaseCompressionResult)(Application.GetService(Of IWikiService).GetAllDatabaseCompressionResultsAsync().GetAwaiter.GetResult)

        FilteredResults = CollectionViewSource.GetDefaultView(DatabaseResults)
        FilteredResults.Filter = AddressOf FilterResults
    End Sub


    Private Function NormalizeString(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty
        Return New String(input.Where(Function(c) Char.IsLetterOrDigit(c) OrElse Char.IsWhiteSpace(c)).ToArray()).ToLowerInvariant()
    End Function

    Private Function FilterResults(obj As Object) As Boolean
        If String.IsNullOrWhiteSpace(SearchText) Then Return True
        Dim item = TryCast(obj, DatabaseCompressionResult)
        If item Is Nothing OrElse item.GameName Is Nothing Then Return False

        ' Normalize  GameName for punctuation-insensitive search
        Dim normalizedGameName = NormalizeString(item.GameName)

        Return (item.GameName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) OrElse
           (normalizedGameName.Contains(SearchText)) OrElse
           (item.SteamID.ToString().Contains(SearchText))
    End Function

    Public ReadOnly Property SortCommand As ICommand = New RelayCommand(Of Object)(Sub(f) SortResults(f))


    Private Sub SortResults(param As Object)
        Dim sortOption = param?.ToString()
        FilteredResults.SortDescriptions.Clear()

        Select Case sortOption
            Case "GameNameAsc"
                FilteredResults.SortDescriptions.Add(New SortDescription("GameName", ListSortDirection.Ascending))
            Case "GameNameDesc"
                FilteredResults.SortDescriptions.Add(New SortDescription("GameName", ListSortDirection.Descending))
            Case "SteamIDAsc"
                FilteredResults.SortDescriptions.Add(New SortDescription("SteamID", ListSortDirection.Ascending))
            Case "SteamIDDesc"
                FilteredResults.SortDescriptions.Add(New SortDescription("SteamID", ListSortDirection.Descending))
            Case "MaxSavingsAsc"
                FilteredResults.SortDescriptions.Add(New SortDescription("MaxSavings", ListSortDirection.Ascending))
            Case "MaxSavingsDesc"
                FilteredResults.SortDescriptions.Add(New SortDescription("MaxSavings", ListSortDirection.Descending))
        End Select
    End Sub


End Class
