Imports System.Linq
Namespace My
    Partial Friend Class MyApplication
        Private Sub BatchStartup(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            ' Keep all settings beside the executable; do not write user.config outside the working directory.
            Me.SaveMySettingsOnExit = False
            If e.CommandLine.Contains("--batch", StringComparer.OrdinalIgnoreCase) Then
                Environment.ExitCode = Batch2604.Run(e.CommandLine.ToArray())
                e.Cancel = True
            End If
        End Sub
    End Class
End Namespace
