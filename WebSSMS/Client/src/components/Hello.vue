<!-- src/components/Hello.vue -->
<template>
    <Split direction="--">
        <split direction="|">
            <div>
                <div>
                    <button @click="run_query" :disabled="is_waiting_for_query">{{ is_running ? 'Cancel' : 'Run'}}</button>
                    <button @click="run_slow_query" :disabled="is_waiting_for_query">{{ is_running ? 'Cancel' : 'Run Slowly'}}</button>
                </div>
                <Editor :onMounted="on_editor_mounted" :onCodeChange="on_code_change"></Editor>
            </div>
            <div>
                <div>
                    <v-select :options="conn_strings" v-model="current_conn_string" label="label" :onChange="load_tables">
    
                    </v-select>
                </div>
                <tables-tree :insertText="insert_text_to_editor"></tables-tree>
            </div>
        </split>
        <div>
            <Split direction="--" is-dynamic>
                <span v-if="query_start_error">
                    {{ query_start_error }}
                </span>
                <div v-else :key="'results' + i" v-for="(query, id, i) in results" class="result-container" :style="{height: result_height}">
                    <div class="result-stats">
                        <div class="query-text">
                            {{ query.SqlText}}
                        </div>
                        <div>
                            Execution time (ms):
                            <b v-if="query.Stats">{{ query.Stats.ExecutionTime}}</b>
                        </div>
                    </div>
                    <div v-if="query.Error"> {{ query.Error }}  </div>
                    <div v-if="is_waiting(query)"> Waiting  </div>
                    <div v-if="is_cancelled(query)"> Cancelled  </div>
                    <ResultsDataTable v-if="query.data && query.data.length" :uid="id + '-table-'" :data="query.data"></ResultsDataTable>
                </div>
    
            </Split>
    
        </div>
    </Split>
</template>

<script lang="ts">
import Vue, { ComponentOptions } from "vue";
import { Http } from "../Utils";
import $ from "jquery";
import Split from "./SplitPanel.vue"
import TablesTree from "./TablesTree.vue"
import Editor from "./Editor.vue"
import ResultsDataTable from "./ResultsDataTable.vue"
import DatabasePanel from "./DatabasePanel.vue"
import vSelect from "vue-select"
import store from "@/Store"
import { SqlQueryStatus} from "@/ISqlQuery"

declare let monaco: any;

interface C extends Vue {
    editor: any
    is_running:boolean
    run_query: Function
}

export default {
    components: {
        Split,
        Editor,
        DatabasePanel,
        vSelect,
        TablesTree,
        ResultsDataTable
    },
    data() {
        return {

        }
    },
    methods: {
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
            let updatePosition = _ => {
                store.commit("setEditorCursorPosition", editor.getPosition())
                let selection = editor.getSelection()
                let selectedText = editor.getModel().getValueInRange(selection)
                store.commit("setEditorSelectedText", selectedText)
            }
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
        }
    },
    computed: {
        conn_strings: _ => store.state.connectionStrings,
        is_running: _ => store.state.queryResults.isRunningQueries,
        current_conn_string: _ => store.state.persistent.currentConnectionString,
        is_waiting_for_query: _ => store.state.queryResults.pre.isWaitingForQuery,
        results: _ => store.state.queryResults.queries,
        result_height: _ => {
            let percents = (100 / (Object.getOwnPropertyNames(store.state.queryResults.queries).length - 1)) + "%"
            return `calc(${percents} - 12px)`
        },
        query_start_error(){
            return this.$store.state.queryResults.pre.error;
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
}
</style>
