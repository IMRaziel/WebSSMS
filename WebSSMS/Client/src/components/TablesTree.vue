<template>
  <div style="overflow: auto; height: 100%" >
    <ul>
      <li :key="t.name" v-for="t in tables" @contextmenu.stop.prevent="show_table_menu">
        <div class="bold" @click="toggle(t.name)">
          {{t.name}}
          <span>[{{is_expanded(t.name) ? '-' : '+'}}]</span>
        </div>
        <ul v-if="is_expanded(t.name)">
          <li :key="f.name + t.name" v-for="f in t.fields">
            {{ f.name }}[{{ f.type }}]
          </li>
        </ul>
      </li>
    </ul>

    <context-menu id="context-menu" ref="ctxMenu">
      <li @click="insert_select_query"> Generate "Select" Query</li>
      <li> Generate "Delete" Query</li>
      <li> Generate "Update" Query</li>
      <li> Generate "Insert" Query</li>
    </context-menu>


  </div>
</template>

<script lang="ts">
import Vue, { ComponentOptions } from "vue";
import { Http } from "../Utils";
import $ from "jquery";
import store from "@/Store"
import contextMenu  from "vue-context-menu"

interface C extends Vue {
  insertText: Function
}

export default {
  props: ["insertText"],
  components: {
    contextMenu
  },
  data() {
    return {

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
    show_table_menu(ev){
      if(ev.srcElement.tagName!="DIV"){ return }
      (<any>this.$refs.ctxMenu).open(ev)
    },
    insert_select_query(){
      this.insertText("SELECT QUERY HERE")
    }
  },
  computed: {
    tables: () => store.state.tables
  }
} as ComponentOptions<C>
</script>

<style>

</style>
