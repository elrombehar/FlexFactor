Some notes:
1. For simplicity, all files are in the same project. For real world, the files will be placed in different projects, 
   where same can be used as nugets. For xample - Models can be a different project/nuget. Exceptions, Infra, interfaces and services can
   also be a different projects. 
2. As for intefaces - some like to specfiy the inteffaces in one location while other prefer to place the interfaeces in the same
   location as the implementation. 
3. There is a seperate tests project. Not 100% covergae
4. There is a typo in the main project - 's' instead of 'c' in the 'DisputeReconsile'.

How to run:
1. Rebuild the solution.
2. Copy the files in sample-files folder to the /bin output folder.
3. run :
   DisputeReconsile.exe --input external.json --output out_json.json
   DisputeReconsile.exe --input external.json --output out_csv.csv
   DisputeReconsile.exe --input external.xml --output out_csv.csv
   DisputeReconsile.exe --input external.csv --output out_csv.csv

Example console output:

info: DisputeReconsile.Program[0]
      Starting Dispute Reconcile
info: DisputeReconsile.Program[0]
      Input file: external.csv
info: DisputeReconsile.Program[0]
      Output file: out_csv.csv
Fetching external disputes
info: DisputeReconsile.Infra.FileHandlers.CsvFileHandler[0]
      Reading CSV file: external.csv
info: DisputeReconsile.Infra.FileHandlers.CsvFileHandler[0]
      Successfully read 5 disputes from CSV
Found 5 external disputes
Fetching internal disputes...
Found 3 internal disputes
Processing...
info: DisputeReconsile.Services.ReconcileService[0]
      Starting dispute reconcile process
[HIGH] High severity discrepancies detected: 0 Critical, 1 High priority issues found during reconcile process
warn: DisputeReconsile.Infra.Alerts.AlertService[0]
      Alert sent: [HIGH] High severity discrepancies detected: 0 Critical, 1 High priority issues found during reconcile process
fail: DisputeReconsile.Infra.Alerts.AlertService[0]
      High severity discrepancies alert: High severity discrepancies detected: 0 Critical, 1 High priority issues found during reconcile process
info: DisputeReconsile.Services.ReconcileService[0]
      Reconciliation completed. Found 5 discrepancies, 1 high-severity

RECONCILE SUMMARY
===================================================
Total External Records: 5
Total Internal Records: 3
Total Discrepancies: 5

DISCREPANCY BREAKDOWN:
Missing in Internal: 3
Missing in External: 1
Status Mismatches: 1
Amount Mismatches: 0

HIGH SEVERITY DISCREPANCIES: 1
 case_002: Status mismatch: External='Won', Internal='Lost'

Processed at: 2025-05-25 07:59:41 UTC
===================================================
Generating report...
info: DisputeReconsile.Infra.FileHandlers.CsvFileHandler[0]
      Writing reconciliation result to: out_csv.csv
info: DisputeReconsile.Infra.FileHandlers.CsvFileHandler[0]
      Successfully wrote reconciliation result
info: DisputeReconsile.Program[0]
      Dispute reconcile completed successfully
Reconcile completed successfully!
Reconcile results written to: out_csv.csv

