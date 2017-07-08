import Vuex from "vuex"
import Vue from "vue"
import { Http } from "@/Utils"
import ISelectModel from "@/ISelectModel"
import { SqlQueryStatus, ISqlQuery, IRunQueryResults} from "@/ISqlQuery"
import { createModule, ADD_TOAST_MESSAGE  } from 'vuex-toast'

Vue.use(Vuex)

let state = {
  save_load : {
    currentName: "",
    currentId: "",
    isSaving: false,
    isLoading: false,
    selectorVisible: false,
    queriesList: [] as ISelectModel[]
  },
  connectionStrings: [] as ISelectModel[],
  isLoadingTables: false,
  tables: [] as { name: string, fields: { name: string, type: string }[] }[],
  persistent: {
    currentConnectionString: <ISelectModel | null> null,
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
    _loadedCode: "",
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


declare let process: any;

export default new Vuex.Store({
  strict: process.env.NODE_ENV !== 'production',
  modules: {
    toast: createModule({
      dismissInterval: 8000
    }),
  },
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
    // connections list
    setCurrentConnString(state, connString) {
      state.persistent.currentConnectionString = connString
    },
    setConnStrings(state, conn_strings) {
      state.connectionStrings = conn_strings
    },

    // tables list
    setTables(state, tables) {
      Vue.set(state, "tables", tables)
    },
    setIsLoadingTables(state, val) {
      state.isLoadingTables = val;
    },
    toggleTableExpanded(state, name) {
      let expandedTables = state.persistent.expandedTables
      if (name in expandedTables) {
        expandedTables[name] = !expandedTables[name]
      } else {
        Vue.set(expandedTables, name, true)
      }
    },

    // code editor
    setEditorCode(state, val) {
      state.editor.code = val;
    },
    setEditorCursorPosition(state, pos) {
      state.editor.cursorPosition.column = pos.column
      state.editor.cursorPosition.lineNumber = pos.lineNumber
    },
    setEditorSelectedText(state, text) {
      state.editor.selection = text
    },
    editQueryText(state, {pos, text}){
      state.editor.cursorPosition.column = pos.column
      state.editor.cursorPosition.lineNumber = pos.lineNumber
      state.editor.selection = text
    }, 

    // running queries
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
      state.queryResults.pre.isWaitingForQuery = false
      
      if(queryResult && queryResult.error){
        state.queryResults.pre.error = queryResult.error
      }
    },

    // save/load
    updateQueryName(state, name: string) {
      state.save_load.currentName = name
    },
    startQueryListLoad(state) {
      state.save_load.queriesList = []
      state.save_load.isLoading = true
      state.save_load.selectorVisible = true
    },
    endQueryListLoad(state, queries){
        state.save_load.isLoading = false
        if (!queries.length){
          state.save_load.selectorVisible = false
        }
        state.save_load.queriesList = queries
    },
    startQuerySave(state){
      state.save_load.isSaving = true
    },
    setCurrentQuery(state, q: ISelectModel){
      state.save_load.currentId = q.id
      state.save_load.currentName = q.label
      state.editor._loadedCode = q.value
      state.save_load.selectorVisible = false
    },
    endQuerySave(){
      state.save_load.isSaving = false
    }

  },
  actions: {
    getConnectionStrings({ commit, dispatch }) {
      Http.get("/api/conn_strings")
        .then(conn_strings => {
          commit('setConnStrings', conn_strings)
        })
        .catch(e=> {
            dispatch(ADD_TOAST_MESSAGE, { text: "Can't load connections list. Try reloading the page", type: "danger", dismissAfter: 10000000})
        })
    },
    loadTables({ commit, state, dispatch }, conn_string_obj) {
      commit('setCurrentConnString', conn_string_obj)
      if (state.persistent.currentConnectionString != null) {
        commit('setIsLoadingTables', true)
        state.isLoadingTables = true;
        Http.get(`/api/tables?conn_string_id=${state.persistent.currentConnectionString.value}`)
          .then(tables => {
            commit('setTables', tables)
            commit('setIsLoadingTables', false)
          })
          .catch(e=> {
              dispatch(ADD_TOAST_MESSAGE, { text: "Can't load tables list. Try reloading the page", type: "danger", dismissAfter: 10000000})
              commit('setCurrentConnString', null)
          })
      }
    },
    cancelQuery({ commit, state, getters }) {
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
    runQuery({ commit, state, getters, dispatch }, slow) {
      function getResults(query){
        Http.get(`/api/query_runner/results?query_id=${query.id}`)
          .then((q: ISqlQuery) => {
            commit("updateQueryResult", q)
            if (getters.allQueriesFinished) {
              commit("finalizeQueryResults")
            }
          })
          .catch(e=> {
              dispatch(ADD_TOAST_MESSAGE, { text: "Unexpected error executing the query.", type: "danger"})
              query.QueryStatus = SqlQueryStatus.Error
              query.Error = "Unexpected error"
              commit("updateQueryResult", query)
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
                getResults(query)
              }
            })
          })
          .catch(e=> {
              dispatch(ADD_TOAST_MESSAGE, { text: "Unexpected error while starting the query", type: "danger", dismissAfter: 10000000})
              commit("finalizeQueryResults", {
                error: "Unexpected error while starting the query"
              })
          })

      }
    },
    loadQueriesList({commit, state, dispatch}){
      commit("startQueryListLoad")
      return Http.get(`/api/query_list`)
        .then((q: ISelectModel[]) => {
          if(!q.length){
            dispatch(ADD_TOAST_MESSAGE, { text: "No saved queries found", type: "info" })
          }
          commit("endQueryListLoad", q)
        })
        .catch(e=> {
            dispatch(ADD_TOAST_MESSAGE, { text: "Unexpected error while loading saved queries list", type: "danger", dismissAfter: 10000000})
            commit("endQueryListLoad", [])
        })
        
    },
    loadQuery({ commit, state, dispatch }, q: ISelectModel) {
      commit("setCurrentQuery", q)
      commit("")
    },
    saveNewQuery({commit, state, dispatch}){
      state.save_load.currentId = ""
      dispatch("saveQuery")
    },
    saveQuery({commit, state, dispatch}){
      if (!state.save_load.currentName){
        dispatch(ADD_TOAST_MESSAGE, { text: "Please enter name before saving", type: "info" })
        return
      }
      
      // reset queries list, so it will load with new changes when requested by user
      commit("endQueryListLoad", [])

      commit("startQuerySave")
      Http.post("/api/query_list/save", {
                value: state.editor.code,
                label: state.save_load.currentName,
                id: state.save_load.currentId
              })
              .then((q) => {
                commit("setCurrentQuery", q)
                commit("endQuerySave")
                dispatch(ADD_TOAST_MESSAGE, { text: "Saved", type: "info" })
              })
              .catch(e=> {
                dispatch(ADD_TOAST_MESSAGE, { text: "Unexpected error while saving a query", type: "danger", dismissAfter: 10000000})
                commit("endQuerySave")
              })
    },
  }
}) 
