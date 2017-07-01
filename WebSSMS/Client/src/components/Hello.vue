<!-- src/components/Hello.vue -->
<template>
    <Split direction="--">
        <split direction="|">
            <Editor></Editor>
            <div>
                <div>
                    <v-select 
                        :options="conn_strings" v-model="current_conn_string" 
                        label="label" :onChange="load_tables">

                    </v-select>
                </div>
                <tables-tree></tables-tree>
            </div>
        </split>
        <div>
            Results here
        </div>
    </Split>
</template>

<script lang="ts">
import Vue from "vue";
import { Http } from "../Utils";
import $ from "jquery";
import Split from "./SplitPanel.vue"
import TablesTree from "./TablesTree.vue"
import Editor from "./Editor.vue"
import DatabasePanel from "./DatabasePanel.vue"
import vSelect from "vue-select"
import store from "@/Store"

async function test(){
    var q = await Http.get("/api/meta")
    return q;
}

export default Vue.extend({
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
        load_tables: function(val){
            store.dispatch('loadTables', val)
        }
    },
    computed: {
        conn_strings: _ => store.state.connectionStrings,
        current_conn_string: _ => store.state.persistent.currentConnectionString
    },
    mounted(){
        store.dispatch('getConnectionStrings')
    }
});
</script>

<style>
.greeting {
    font-size: 20px;
}
</style>
