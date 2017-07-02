<!-- src/components/Hello.vue -->
<template>
    <Split direction="--">
        <split direction="|">
            <Editor :onMounted="on_editor_mounted" :onCodeChange="on_code_change" ></Editor>
            <div>
                <div>
                    <v-select 
                        :options="conn_strings" v-model="current_conn_string" 
                        label="label" :onChange="load_tables">

                    </v-select>
                </div>
                <tables-tree :insertText="insert_text_to_editor"></tables-tree>
            </div>
        </split>
        <div>
            Results here
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
import DatabasePanel from "./DatabasePanel.vue"
import vSelect from "vue-select"
import store from "@/Store"

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
        TablesTree
    },
    data() {
        return {
        }
    },
    methods: {
        load_tables(val){
            store.dispatch('loadTables', val)
        },
        on_code_change(editor){
            store.commit("setEditorCode", editor.getValue())
        },
        on_editor_mounted(editor){
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
        insert_text_to_editor(text: string){
            let editor = this.editor;
            var line = editor.getPosition();
            var range = new monaco.Range(line.lineNumber, line.column, line.lineNumber, line.column);
            var id = { major: 1, minor: 1 };
            var op = { identifier: id, range: range, text: text, forceMoveMarkers: true };
            editor.executeEdits("my-source", [op]);
        }
    },
    computed: {
        conn_strings: _ => store.state.connectionStrings, 
        current_conn_string: _ => store.state.persistent.currentConnectionString
    },
    mounted(){
        store.dispatch('getConnectionStrings')
    }
} as ComponentOptions<C>
</script>

<style>
.greeting {
    font-size: 20px;
}
</style>
