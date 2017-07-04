<!-- src/components/Hello.vue -->
<template>
    <Split direction="--">
        <split direction="|">
            <div>
                <div>
                    <button @click="run_query">Run</button>
                    <button @click="run_slow_query">Run Slowly</button>
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
                <div :key="'results' + i" v-for="(query, id, i) in results" class="result-container" :style="{height: result_height}">
                    <ResultsDataTable :uid="id + '-table-'" :data="query.data"></ResultsDataTable>
    
                    <!--<div class="result-stats">
                            <div v-if="query.QueryStatus==0">
                                <button @click="cancel_query(query.id)" >Cancel</button>
                            </div>
                            <div class="query-text">
                                 {{ query.SqlText}}
                            </div>
                            <div>
                                Execution time (ms): <b v-if="query.Stats">{{ query.Stats.ExecutionTime}}</b> 
                            </div>
                        </div>
                        <div class="result-table">
                            <div v-if="query.QueryStatus==3">
                                Canceled
                            </div>
        
                            <ResultsDataTable :data="query.data" v-else></ResultsDataTable>
                        </div>
                        -->
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
import { TSParser } from 'TSParser'

let w = <any>window;
w.sqlParser = TSParser


declare let monaco: any;

interface C extends Vue {
    editor: any
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
        run_query() {
            console.log(2)
            store.dispatch('runQuery')
        },
        run_slow_query() {
            store.dispatch('runQuery', true)
        },
        cancel_query(id) {
            store.dispatch('cancelQuery', id)
        }
    },
    computed: {
        conn_strings: _ => store.state.connectionStrings,
        current_conn_string: _ => store.state.persistent.currentConnectionString,
        results: _ => store.state.queryResults.queries,
        result_height: _ => {
            let percents = (100 / (Object.getOwnPropertyNames(store.state.queryResults.queries).length - 1)) + "%"
            return `calc(${percents} - 12px)`
        }
    },
    mounted() {
        store.dispatch('getConnectionStrings')
    }
} as ComponentOptions<C>
</script>

<style>
.result-stats {
    height: 20px;
}

.result-table {
    height: calc(100% - 20px);
    overflow: auto
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


input[type=range][orient=vertical] {
    writing-mode: vertical-rl;
    /*direction: rtl;*/
    /* IE */
    -webkit-appearance: slider-vertical;
    /* WebKit */
    width: 100%;
    height: 100%;
}
</style>
