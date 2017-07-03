# Build instructions
Server: WebApi project is in ```./WebSSMS```

Client: webpack project in ```./WebSSMS/Client```

## Run client dev server
```
cd /WebSSMS/Client
npm cache clean
npm install
npm run dev
```
It will launch dev server at ```http://localhost:8080```

You can change port at ```./WebSSMS/Client/config/index.js #L26```

## Build client 
```
npm run build
```
It will compile everything and place static files to ```./WebSSMS/Client/dist```

## Run Server

Run fron Visual Studio. It should start web api and open client dev server page in browser.

Server url for client is stored in ```Utils.URL_ROOT```



# Project Description

Write a javascript/typescript editor that works like microsoft sql management studio query editor. It will call a backend REST service that will

- provide meta data (table names. field names)

- execute queries and provide results

- allow saving/loading of prior executions

The editor will have the following features

- allow selection of connection (this will be a list provided by rest service)

- (right side) have a tree view that shows all table names, and when expanded shows all field names (with field type in [] after). A button will allow copying of table name and/field name into spot where cursor is in editor. Double clicking on field also adds it to current location

- option to add all fields for table (right click menu)

- option to build insert statement (right click on menu)

- option to build delete statement (right click on menu)

- option to build select statement (right click on menu)

- allow running of query and displaying results in table(s) at bottom

- messages dialog that displays counts of query runs

- ability to save (will be service call)

- ability to save as file

- ability to load from file

- ability to load previosly saved

- respect commented lines

- syntax highlight (color) commented lines so clear they will not be executed

- if there is a text selection, only run selected part

- show timings of running each part of query in message area

- allow canceling of query

You will be responsible for

- implementation of backend service

- implementation and testing of UI

Attached is an example interface for what the backend service needs to do:

```csharp
public interface IQueryService {
  string[] GetConnectionNames();
  string[] GetTableNames(string connectionName);
  FieldInfo[] GetFields(string connectionName, string tableName);
  string GetSavedQueryList();
  string GetSavedQuery(string name);
  void SaveQuery(string name, string query);
  QueryReturn[] RunQuery(string connectionName, string query);
  string[] GetAsCSV(QueryReturn data);
}

public class FieldInfo {
  public string FieldName { get; set; }
  public string FieldType { get; set; }
}

public class QueryReturn
{
// alot of data here...
}
```