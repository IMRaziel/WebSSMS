<template>
  <div class="tree-container" >
    <ul>
      <li :key="t.name" v-for="t in tables" @contextmenu.stop.prevent="show_table_menu($event, t)">
        <div>
          <span @click="toggle(t.name)" :style="{cursor: 'pointer', 'font-weight': 'bold'}" title="Show/hide fields">[{{is_expanded(t.name) ? '-' : '+'}}]</span>
          <span @click="insert_table_name(t.name)" :style="{cursor: 'pointer', 'font-weight': 'bold'}" title="Insert table name at cursor position">[<]</span>
          <span @dblclick="insert_table_name(t.name)" :style="{cursor: 'pointer'}" class="tname">{{t.name}}</span>
        </div>
        <ul v-if="is_expanded(t.name)">
          <li :key="f.name + t.name" v-for="f in t.fields">
            <span @click="insert_field_name(t.name, f.name)" :style="{cursor: 'pointer', 'font-weight': 'bold'}" title="Insert field name at cursor position">[<]</span>
            <span @dblclick="insert_field_name(t.name, f.name)" :style="{cursor: 'pointer'}">
              {{ f.name }}[{{ f.type }}]
            </span>
          </li>
        </ul>
      </li>
    </ul>

    <context-menu id="context-menu" ref="ctxMenu" :style="{'border-bottom': 'solid 1px'}">
      <li @click="insert_select_query" :style="{'border-bottom': 'solid 1px', padding: '5px', cursor: 'pointer'}"> Generate "Select" Query</li>
      <li @click="insert_delete_query" :style="{'border-bottom': 'solid 1px', padding: '5px', cursor: 'pointer'}"> Generate "Delete" Query</li>
      <li @click="insert_update_query" :style="{'border-bottom': 'solid 1px', padding: '5px', cursor: 'pointer'}"> Generate "Update" Query</li>
      <li @click="insert_insert_query" :style="{'border-bottom': 'solid 1px', padding: '5px', cursor: 'pointer'}"> Generate "Insert" Query</li>
      <li @click="insert_all_fields" :style="{'border-bottom': 'solid 1px', padding: '5px', cursor: 'pointer'}"> Generate All Fields</li>

    </context-menu>


  </div>
</template>

<script lang="ts">
import Vue, { ComponentOptions } from "vue";
import { Http } from "../Utils";
import $ from "jquery";
import store from "@/Store"
import contextMenu  from "vue-context-menu"

interface Table {name: string, fields: Field[]}
interface Field {name: string, type:string}

interface C extends Vue {
  insertText: Function,
  tables: Table[],
  currentTable: Table
}

export default {
  props: ["insertText"],
  components: {
    contextMenu
  },
  data() {
    return {
      currentTable: null
    }
  },
  methods: {
    on_scroll(e){
      console.log(2, e)
    },
    is_expanded(table_name: string) {
      return store.state.persistent.expandedTables[table_name]
    },
    toggle: function (name: string) {
      store.commit('toggleTableExpanded', name)
    },
    show_table_menu(ev, t: Table){
      // only show menu if clicked on table name
      if(ev.srcElement.className!="tname"){ return }

      this.currentTable = t;
      (<any>this.$refs.ctxMenu).open(ev)
    },
    insert_select_query(){
      let table = this.currentTable; 
      if(!table) return
      let allFields = table.fields.map(x=>x.name).join("\n    ,")
      this.insertText(`SELECT TOP (1000) ${allFields} FROM ${table.name}\n`)
    },
    insert_all_fields(){
      let table = this.currentTable; 
      if(!table) return
      let allFields = table.fields.map(x=>x.name).join("\n    ,")
      this.insertText(allFields)
    },
    insert_delete_query(table_name){
      let table = this.currentTable; 
      if(!table) return
      let allFields = table.fields.map(x=>x.name).join("\n    ,")
      this.insertText(`DELETE FROM ${table.name} \nWHERE <Search Conditions,,>\n`) 
    },
    insert_update_query(table_name){
      let table = this.currentTable; 
      if(!table) return
      let allFields = table.fields.map(x=>`${x.name} = <${x.name}>`).join("\n    ,")
      this.insertText(`UPDATE ${table.name} \nSET ${allFields} \nWHERE <Search Conditions,,>\n`) 
    },
    insert_insert_query(table_name){
      let table = this.currentTable; 
      if(!table) return
      var d = "\n    ,"
      let allFields = table.fields.map(x=>x.name).join(d)
      let allFieldValues = table.fields.map(x=>`<${x.name}>`).join(d)
      this.insertText(`INSERT INTO ${table.name} \n    (${allFields})\nVALUES \n    (${allFieldValues})\n`) 
    },
    insert_field_name(table_name:string, field_name){
      let parts = table_name.split("].[")
      let last = parts[parts.length - 1]
      this.insertText(",[" + last + "." + field_name) 
    },
    insert_table_name(table_name:string){
      this.insertText(table_name) 
    },
  },
  computed: {
    tables: () => store.state.tables
  }
} as ComponentOptions<C>
</script>

<style lang="scss" scoped>
ul {
  list-style: none;
  padding-left: 0px;
  padding-bottom: 8px;
  padding-top: 8px;
  li {
    border-bottom: solid lightgrey 1px;
    padding-left: 20px;
  }
}

.tree-container {
  height: calc(100% - 20px);
  overflow: auto;
}
</style>
