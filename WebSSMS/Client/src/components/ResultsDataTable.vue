<template>
  <table>
    <thead>
      <tr>
        <th :key="'col'-key" v-for="key in columns">
          {{ key }}
        </th> 
      </tr>
    </thead>
    <tbody>
      <tr :key="id(row)" v-for="row in rows">
        <td :key="f" v-for="f in row">
          {{ f }}
        </td>
      </tr>
    </tbody>
  </table>
</template>

<script lang="ts">
import Vue, { ComponentOptions } from "vue";
import { Http } from "../Utils";
import $ from "jquery";

interface C extends Vue {
  data: {}[]
  rows: {}[],
  columns: string[]
}

export default {
  props: ["data"],
  components: {
  },
  data() {
    return {

    }
  },
  methods: {
    id(row: {}){
      return Math.random()
    }
  },
  computed: {
    columns(){
      return this.data ? Object.getOwnPropertyNames(this.data[0]).filter(x=>x!="__ob__") : []
    },
    rows(){
      return (this.data || [])
              .map(obj => this.columns.map(c=> obj[c]))
    }
  }
} as ComponentOptions<C>
</script>

<style scoped>
table {
  height: 100%
}


tbody tr:nth-child(odd) {
  background-color: #ccc;
}

</style>
