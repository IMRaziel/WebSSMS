import Vuex from "vuex"
import Vue from "vue"
import { Http } from "@/Utils"
import { SqlQueryStatus, ISqlQuery} from "@/ISqlQuery"
Vue.use(Vuex)

let state = {
  connectionStrings: [] as { label: string, value: string }[],
  isLoadingTables: false,
  tables: [] as { name: string, fields: { name: string, type: string }[] }[],
  persistent: {
    currentConnectionString: <{ value: string, label: string } | null>null,
    expandedTables: {} as { [k: string]: boolean }
  },
  editor: {
    cursorPosition: {
      column: 0,
      lineNumber: 0
    },
    code: "",
    selection: ""
  },
  queryResults: {
    queries: {} as { [k: string]: ISqlQuery },
    isRunningQueries: false,
    pre: {
      isWaitingForQueryConfigs: false,
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
    resetQueryResults(state){
      state.queryResults.queries = {}
      state.queryResults.isRunningQueries = true
      state.queryResults.pre.error = ""
      state.queryResults.pre.isWaitingForQueryConfigs = true
    },
    updateQueryResult(state, queryResult: ISqlQuery){
      state.queryResults.pre.isWaitingForQueryConfigs = false
      Vue.set(state.queryResults.queries, queryResult.id, queryResult)
    },
    finalizeQueryResults(state){
      state.queryResults.isRunningQueries = false
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
    async cancelQuery({ commit, state, getters }, id: string) {
      Http.get(`/api/query_runner/cancel?query_id=${id}`)
        .then((query: ISqlQuery) => {
          commit("updateQueryResult", query)
          if (getters.allQueriesFinished) {
            commit("finalizeQueryResults")
          }
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
            if (q.NextQuery){
              commit("updateQueryResult", q.NextQuery)
              getResults(q.NextQuery.id)
            }
          })
      }


      commit("resetQueryResults")
      slow = !!slow
      let cs = state.persistent.currentConnectionString
      if (cs != null) {
        Http.post("/api/query_runner/run", {
          conn_string_id: cs.value,
          query_text: state.editor.selection || state.editor.code,
          slow
        })
          .then((query: ISqlQuery) => {
            commit("updateQueryResult", query)
            if (query.QueryStatus == SqlQueryStatus.Running){
              getResults(query.id)
            }
          })
      }
    }
  }
})
