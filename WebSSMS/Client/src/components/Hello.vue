<!-- src/components/Hello.vue -->
<template>
    <Split direction="--">
        <split direction="|">
            <div>
                <div>
                    <toast position="ne"></toast>
                    
                    <input type="test" placeholder="Query Name" v-model="query_name"></input>
                    <button @click="saveQuery">Save</button>
                    <button @click="saveNewQuery">Save as new</button>
                    <button @click="show_saved_queries_list"> {{ sl.selectorVisible ? "Hide list" : "Load" }} </button>
                    <v-select v-show="sl.selectorVisible" ref="queries_list_selector" placeholder="Queries list" :options="sl.queriesList" label="label" @search="load_queries_list" :on-change="select_query">
                    </v-select>

                </div>
                <div>
                    <button @click="run_query" :disabled="is_waiting_for_query || !$store.state.persistent.currentConnectionString">
                        {{ is_running ? 'Cancel' : 'Run'}}
                    </button>
                    <button @click="run_slow_query" :disabled="is_waiting_for_query  || !$store.state.persistent.currentConnectionString">
                        {{ is_running ? 'Cancel' : 'Run Slowly'}}
                    </button>
                </div>
                <Editor :onMounted="on_editor_mounted" :onCodeChange="on_code_change"></Editor>
            </div>
            <div>
                <div>
                    <v-select placeholder="Select connection" :options="conn_strings" label="label" :onChange="load_tables">
    
                    </v-select>
                </div>
                <tables-tree :insertText="insert_text_to_editor"></tables-tree>
            </div>
        </split>
        <div>
            <Split direction="--" is-dynamic>
                <span v-if="query_start_error" class="error centered">
                    {{ query_start_error }}
                </span>
                <result-panel v-else :key="'results-group' + i" v-for="(group, i) in result_groups" class="result-container" :style="{height: result_group_height}">
                    <result-panel :key="'results' + i" v-for="(query, i) in group" :style="{height: result_height(group, i)}">
                        <div class="result-stats">
                            <div class="query-text" :title="query.SqlText">
                                {{ query.SqlText}}
                            </div>
                            <div :title="JSON.stringify(query.Stats)">
                                <a 
                                    v-if="query.SqlText.startsWith('SELECT')" 
                                    :href="URL_ROOT + '/api/query_runner/download_as_csv?query_id=' + query.id + '&name=' + (query_name ? query_name : 'query_result') + '.csv'" download>
                                    Download as CSV
                                </a>
                                <span>
                                    Execution time (ms):
                                    <b v-if="query.Stats">{{ query.Stats.ExecutionTime}}</b>
                                </span>
                                <span>
                                    Selected Rows:
                                    <b v-if="query.Stats">{{ query.Stats.SelectRows}}</b>
                                </span>
                            </div>
                        </div>
                        <div v-if="query.Error" class="error centered"> {{ query.Error }}  </div>
                        <div v-if="is_waiting(query)" class="centered"> Waiting  </div>
                        <div v-if="is_cancelled(query)" class="error centered"> Cancelled  </div>
                        <ResultsDataTable v-if="query.data && query.data.length" :uid="id + '-table-'" :data="query.data"></ResultsDataTable>
                    </result-panel>
                </result-panel>
    
            </Split>
    
        </div>
    </Split>
</template>

<script lang="ts">
import Vue, { ComponentOptions } from "vue";
import { Http, URL_ROOT, debounce } from "../Utils";
import $ from "jquery";
import Split from "./SplitPanel.vue"
import TablesTree from "./TablesTree.vue"
import Editor from "./Editor.vue"
import ResultsDataTable from "./ResultsDataTable.vue"
import DatabasePanel from "./DatabasePanel.vue"
import vSelect from "vue-select"
import store from "@/Store"
import { SqlQueryStatus, ISqlQuery} from "@/ISqlQuery"
import  {mapState, mapActions} from "vuex"
import { Toast } from 'vuex-toast'
import 'vuex-toast/dist/vuex-toast.css'

declare let monaco: any;

interface C extends Vue {
    editor: any
    is_running:boolean
    run_query: Function
    results: ISqlQuery[],
    result_groups: ISqlQuery[][],
}

let ResultPanel = Vue.component("result-panel", {
    template: `
    <div style="border: solid 1px; padding: 4px">
      <slot></slot>
    </div>
    `
});

export default {
    components: {
        Split,
        Editor,
        DatabasePanel,
        vSelect,
        TablesTree,
        ResultsDataTable,
        Toast,
        ResultPanel
    },
    data() {
        return {
            URL_ROOT
        }
    },
    methods: {
        ...mapActions([
            "saveQuery",
            "saveNewQuery",
        ]),        
        select_query(val){
            store.commit("setCurrentQuery", val)
        },
        load_queries_list: (s, loading) => {
            loading(true)
        },
        show_saved_queries_list() {
            let f: any = this.$refs["queries_list_selector"]
            if(this.$store.state.save_load.selectorVisible){
                // hide selector and clear loaded list of saved  queries
                store.commit("endQueryListLoad", [])
                f.deselect()
                return
            }

            f.loading = true
            store
                .dispatch('loadQueriesList')
                .then(_ => {
                    f.open = true
                    f.loading = false
                })
        },
        queries_list: () => [],
        save_query: () => [],
        
        is_cancelled: query => query.QueryStatus == SqlQueryStatus.Cancelled,
        is_waiting: query => query.QueryStatus == SqlQueryStatus.Running,
        load_tables(val) {
            store.dispatch('loadTables', val)
        },
        on_code_change(editor) {
            store.commit("setEditorCode", editor.getValue())
        },
        on_editor_mounted(editor) {
            (<any>window).ed = editor

            this.editor = editor;
            let updatePosition = debounce(_ => {
                let selection = editor.getSelection()
                let selectedText = editor.getModel().getValueInRange(selection)
                store.commit("editQueryText", {pos: editor.getPosition(), text: selectedText})
            }, 500)
            editor.onMouseUp(updatePosition)
            editor.onKeyUp(updatePosition)
        },
        insert_text_to_editor(text: string) {
            let editor = this.editor;
            var line = editor.getPosition();
            var range = new monaco.Range(line.lineNumber, line.column, line.lineNumber, line.column);
            var id = { major: 1, minor: 1 };
            var op = { identifier: id, range: range, text: text, forceMoveMarkers: true };
            editor.executeEdits("my-source", [op]);
        },
        run_query(e, slow:boolean) {
            if(this.is_running) {
                store.dispatch('cancelQuery')
            }   else {
                store.dispatch('runQuery', slow)
            }
        },
        run_slow_query(e) {
            this.run_query(e, true)
        },
        cancel_query(id) {
            store.dispatch('cancelQuery', id)
        },
        result_height(group: ISqlQuery[], i: number) {
            let h = 60
            let is_select = i == group.length - 1 
            if(is_select){
                return `calc(100% - ${(group.length - 1) * (h + 12)}px)`
            }   else {
                return h + "px" 
            }
        },
    },
    computed: {
        ...mapState<typeof store.state>({
            sl: s => s.save_load,
            conn_strings: s => s.connectionStrings,
            is_running: s => s.queryResults.isRunningQueries,
            current_conn_string: s => s.persistent.currentConnectionString,
            is_waiting_for_query: s => s.queryResults.pre.isWaitingForQuery,
            results: s => s.queryResults.queries,
            result_groups: s => {
                var groups: ISqlQuery[][] = [];
                var currGroup:ISqlQuery[] = []
                var qs = s.queryResults.queries
                for (var key in qs) {
                    if (qs.hasOwnProperty(key)) {
                        var q = qs[key]
                        currGroup.push(q)
                        if(q.SqlText.startsWith("SELECT")){
                            groups.push(currGroup)
                            currGroup = []
                        }   
                    }
                }
                if(currGroup.length){
                    groups.push(currGroup)
                }
                return groups
            },
        }),
        result_group_height() {
            let percents = (100 / (Object.getOwnPropertyNames(this.result_groups).length - 1)) + "%"
            return `calc(${percents} - 12px)`
        },
        query_start_error(){
            return this.$store.state.queryResults.pre.error;
        },
        query_name: {
            get() {
                return store.state.save_load.currentName
            },
            set(value) {
                let self = this as any
                if(!self.$query_name_setter){
                    self.$query_name_setter = debounce(v =>{
                        self.$store.commit('updateQueryName', v)
                    }, 500)
                }
                self.$query_name_setter(value);
            }
        }
    },
    mounted() {
        store.dispatch('getConnectionStrings')
    }
} as ComponentOptions<C>
</script>

<style scoped>
.result-stats {
    height: 40px;
}


.result-container {
    height: calc(100% - 18px);
    margin: 4px;
    border-width: 1px;
    border: solid #000;
}


.query-text {
    text-overflow: ellipsis;
    white-space: nowrap;
    overflow: hidden;
}

.error {
    color: red;
}

.centered {
    position: relative;
    float: left;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
}
</style>
