# FlexFactor

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


