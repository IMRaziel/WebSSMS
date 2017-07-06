import Vuex from "vuex"
import Vue from "vue"
import { Http } from "@/Utils"
import { SqlQueryStatus, ISqlQuery, IRunQueryResults} from "@/ISqlQuery"
Vue.use(Vuex)

let state = {
  connectionStrings: [] as { label: string, value: string }[],
  isLoadingTables: false,
  tables: [] as { name: string, fields: { name: string, type: string }[] }[],
  persistent: {
    currentConnectionString: <{ value: string, label: string } | null> null,
    expandedTables: {} as { [k: string]: boolean }
  },
  editor: {
    cursorPosition: {
      column: 0,
      lineNumber: 0
    },
    code: `
    
        SELECT TOP (1000) [OrderID]
                ,[CustomerID]
                ,[EmployeeID]
                ,[OrderDate]
                ,[RequiredDate]
                ,[ShippedDate]
                ,[ShipVia]
                ,[Freight]
                ,[ShipName]
                ,[ShipAddress]
                ,[ShipCity]
                ,[ShipRegion]
                ,[ShipPostalCode]
                ,[ShipCountry]
            FROM [Northwind].[dbo].[Orders]

update [Northwind].[dbo].[Orders] set shipname = '2' where orderid = 10248

        SELECT TOP (1000) [OrderID]
                ,[CustomerID]
                ,[EmployeeID]
                ,[OrderDate]
                ,[RequiredDate]
                ,[ShippedDate]
                ,[ShipVia]
                ,[Freight]
                ,[ShipName]
                ,[ShipAddress]
                ,[ShipCity]
                ,[ShipRegion]
                ,[ShipPostalCode]
                ,[ShipCountry]
            FROM [Northwind].[dbo].[Orders]
      
    `,
    selection: ""
  },
  queryResults: {
    queries: {} as { [k: string]: ISqlQuery },
    isRunningQueries: false,
    pre: {
      isWaitingForQuery: false,
      error: ""
    }
  } 
}



export default new Vuex.Store({
  state,
  getters: {
    allQueriesFinished: state => {
      let qs = state.queryResults.queries
      for(var k in qs){
        if(qs[k].QueryStatus == SqlQueryStatus.Running) return false
      }
      return true
    }
  },
  mutations: {
    setCurrentConnString(state, connString) {
      state.persistent.currentConnectionString = connString
    },
    setConnStrings(state, conn_strings) {
      state.connectionStrings = conn_strings
    },
    setTables(state, tables) {
      Vue.set(state, "tables", tables)
    },
    setIsLoadingTables(state, val) {
      state.isLoadingTables = val;
    },
    setEditorCode(state, val) {
      state.editor.code = val;
    },
    toggleTableExpanded(state, name) {
      let expandedTables = state.persistent.expandedTables
      if (name in expandedTables) {
        expandedTables[name] = !expandedTables[name]
      } else {
        Vue.set(expandedTables, name, true)
      }
    },
    setEditorCursorPosition(state, pos) {
      Vue.set(state.editor, "cursorPosition", pos)
    },
    setEditorSelectedText(state, text) {
      state.editor.selection = text
    },
    startQuery(state){
      state.queryResults.queries = {}
      state.queryResults.isRunningQueries = true
      state.queryResults.pre.error = ""
      state.queryResults.pre.isWaitingForQuery = true
    },
    updateQueryResult(state, queryResult: ISqlQuery){
      state.queryResults.pre.isWaitingForQuery = false
      Vue.set(state.queryResults.queries, queryResult.id, queryResult)
    },
    startQueryCancel(state, queryResult: IRunQueryResults){
        state.queryResults.pre.isWaitingForQuery = true
    },
    finalizeQueryResults(state, queryResult: IRunQueryResults){
      state.queryResults.isRunningQueries = false
      if(queryResult && queryResult.error){
        state.queryResults.pre.error = queryResult.error
      }
    }
  },
  actions: {
    getConnectionStrings({ commit }) {
      Http.get("/api/conn_strings")
        .then(conn_strings => {
          commit('setConnStrings', conn_strings)
        })
    },
    loadTables({ commit, state }, conn_string_obj) {
      commit('setCurrentConnString', conn_string_obj)
      if (state.persistent.currentConnectionString != null) {
        commit('setIsLoadingTables', true)
        state.isLoadingTables = true;
        Http.get(`/api/tables?conn_string_id=${state.persistent.currentConnectionString.value}`)
          .then(tables => {
            commit('setTables', tables)
            commit('setIsLoadingTables', false)
          })
      }
    },
    async cancelQuery({ commit, state, getters }) {
      Object.keys(state.queryResults.queries)
      .filter(x => state.queryResults.queries[x].QueryStatus == SqlQueryStatus.Running)
      .forEach(id => {
        Http.get(`/api/query_runner/cancel?query_id=${id}`)
          .then((query: ISqlQuery) => {
            commit("updateQueryResult", query)
            if (getters.allQueriesFinished) {
              commit("finalizeQueryResults")
            }
          })
      })
    },
    async runQuery({ commit, state, getters }, slow) {
      function getResults(id){
        Http.get(`/api/query_runner/results?query_id=${id}`)
          .then((q: ISqlQuery) => {
            commit("updateQueryResult", q)
            if (getters.allQueriesFinished) {
              commit("finalizeQueryResults")
            }
          })
      }


      commit("startQuery")
      slow = !!slow
      let cs = state.persistent.currentConnectionString
      if (cs != null) {
        Http.post("/api/query_runner/run", {
          conn_string_id: cs.value,
          query_text: state.editor.selection || state.editor.code,
          slow
        })
          .then((queryResult: IRunQueryResults) => {
            if(queryResult.error){
              commit("finalizeQueryResults", queryResult)
              return;
            }
            
            queryResult.queries.forEach(query=> {
              commit("updateQueryResult", query)
              if (query.QueryStatus == SqlQueryStatus.Running){
                getResults(query.id)
              }
            })
          })
      }
    }
  }
})
