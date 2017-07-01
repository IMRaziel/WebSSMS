import Vuex from "vuex"
import Vue from "vue"
import {Http} from "@/Utils"

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    connectionStrings: [],
    isLoadingTables: false,
    tables: [],
    persistent: {
      currentConnectionString: <{ value: string, label: string } | null>null,
      expandedTables: {} as {[k: string]: boolean}
    }
  },
  mutations: {
    setCurrentConnString(state, connString){
      state.persistent.currentConnectionString = connString
    },
    setConnStrings(state, conn_strings){
      state.connectionStrings = conn_strings
    },
    setTables(state, tables) {
      Vue.set(state, "tables", tables)
    },
    setIsLoadingTables(state, val){
      state.isLoadingTables = val;
    },
    toggleTableExpanded(state, name){
      let expandedTables = state.persistent.expandedTables
      if (name in expandedTables) {
        expandedTables[name] = !expandedTables[name]
      } else {
        Vue.set(expandedTables, name, true)
      }

    }
  },
  actions: {
    getConnectionStrings({commit}){
      Http.get("/api/conn_strings")
        .then(conn_strings => {
          commit('setConnStrings', conn_strings)
        })
    },
    loadTables({commit, state}, conn_string_obj){
      commit('setCurrentConnString', conn_string_obj)
      if (state.persistent.currentConnectionString != null){
        commit('setIsLoadingTables', true)
        state.isLoadingTables = true;
        Http.get(`/api/tables?conn_string_id=${state.persistent.currentConnectionString.value}`)
          .then(tables => {
            commit('setTables', tables)
            commit('setIsLoadingTables', false)
          })
      }
    }
  }
})
