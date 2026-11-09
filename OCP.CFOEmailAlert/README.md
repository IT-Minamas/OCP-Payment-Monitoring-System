# OCP CFO Email Alert

A standalone .NET 8 console application for Windows Server. It reads `dbo.vw_rpt_MemoProgress` and emails the matching memo list.

## Business rule

```sql
SELECT fldNo, MemoStatus
FROM dbo.vw_rpt_MemoProgress
WHERE ApprovedOn_L30 IS NOT NULL
  AND ApprovedOn_L40 IS NULL
ORDER BY ApprovedOn_L30, fldNo;
```

The email is sent to `rizky.y.pratomo@gmail.com` by default. The recipient can be changed in `appsettings.json`.

## 1. Configure

Edit `appsettings.json`:

- `Database.ConnectionString`: SQL Server connection to the database containing `dbo.vw_rpt_MemoProgress`.
- `Email.SmtpHost`, `SmtpPort`, `EnableSsl`: SMTP relay settings supplied by your infrastructure/email team.
- `Email.From`: a permitted sender address on the SMTP relay.
- `Email.Username` and `Email.Password`: fill only if the relay requires SMTP authentication. Prefer a dedicated service account and protect this file with Windows ACLs; do not commit real credentials to source control.
- `Email.To`: defaults to `rizky.y.pratomo@gmail.com`.

The app does not bypass TLS certificate validation. If SSL/TLS is enabled, use the SMTP server hostname matching its certificate and ensure the server trusts the issuing CA.

## 2. Build and publish on a development machine

Install the .NET 8 SDK, then run from this folder:

```powershell
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -o .\publish
```

Copy the complete `publish` folder to the Windows Server. Keep `appsettings.json` beside the executable. If you publish again, preserve the configured production `appsettings.json` rather than overwriting it with placeholders.

## 3. Test manually on the server

Run:

```powershell
.\OCP.CFOEmailAlert.exe
```

Check the console output and confirm that the email is received. The application sends an email even when no rows match; in that case the email states that there are no memos pending CFO approval.

## 4. Schedule every Monday at 10:00 AM

Open PowerShell as Administrator in the deployed project folder and run:

```powershell
.\Scripts\Register-WeeklyTask.ps1
```

The script registers a weekly Monday 10:00 AM task. In **Task Scheduler → Task Scheduler Library → OCP CFO Email Alert → Properties → General**, select:

- Run whether user is logged on or not.
- Run with highest privileges only if required by your server policy.
- Change the account to an approved service account that has SQL Server read access and SMTP relay permission.

The service account must have read access to the database/view and permission to send through the configured SMTP relay. Configure the task's stored credentials through Task Scheduler according to your organization's policy.

If the requirement is actually every day at 10:00 AM rather than Monday only, change the trigger in the script to:

```powershell
$trigger = New-ScheduledTaskTrigger -Daily -At $StartTime
```

## 5. Operational notes

- Windows Task Scheduler runs the executable; no Windows Service or continuously running process is required.
- Task Scheduler history and Last Run Result can be used to diagnose task execution. The process returns exit code `0` on success and `1` on failure.
- Ensure the SQL Server is reachable from the Windows Server and the SQL login has only the required `SELECT` permission.
- The app currently sends the current result set each time it runs. It does not maintain a sent-history table or suppress repeat notifications if the same memos remain pending in the next week's run.
- Confirm the SQL schema/database name and SMTP relay settings before production deployment.
