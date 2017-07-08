<template>
  <div style="height: 100%; overflow: hidden" @wheel="on_scroll">
    <div class="container" style="display: inline;">
      <div style=" overflow: hidden; display: inline-block; background-color: gray; width:calc(100% - 20px); height: calc(100% - 60px)" @scroll="on_scroll">
        <div style="height: 100%; float: left;">
          <table class="rownums-table" cellspacing=0>
              <thead>
                <tr>
                  <td >
                    <div class="rownum-cell">#</div>
                  </td>
                </tr>
              </thead>
              <tbody>
                <tr :key="'col'-key" v-for="key in row_nums">
                  <td > 
                    <div class="rownum-cell">{{ key }} </div>
                  </td>
                </tr>
              </tbody>
          </table>
        </div>
        <div class="table-container">
          <table class="data-table" :style="{left: -10 * scrollx + 'px'}" ref="table" cellspacing="0">
            <thead>
              <tr >
                <!--<td class="headcol">1</td>-->
                <th :key="'col'-key" v-for="key in columns">
                  <div class="cell">
                    {{ key }}
                  </div>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr :key="uid + i" v-for="(row, i) in visible_rows" >
                <td :key="(uid + i) + j" v-for="(f, j) in row" >
                  <div class="cell" :title="f">
                    {{ f }}
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div class="scrollx-container">
        <vue-slider ref="qwe"
              height="100%" 
              :width="10"
               direction="vertical" 
               show
               :interval="0.1"
               :speed="0"
               tooltip="never"
              :piecewise="false"
              :process-style="{background: 'transparent'}"
              :style="{ 'display': 'inline-block', 'marginLeft': '-6px' }" 
              v-model.number="scrolly"  ></vue-slider>
      </div>
    </div>
    <vue-slider 
        :interval="0.1" 
        :speed="0" tooltip="never" 
        style="width: calc(100% - 20px); height: 20px; margin-top: -4px" 
        v-model.number="scrollx"></vue-slider>
  </div>
</template>

<script lang="ts">
declare let require: any
import Vue, { ComponentOptions } from "vue";
import { Http } from "../Utils";
import $ from "jquery";
// import  "jquery.datatables"

import VueScroll from "vue-scroll"
import {throttle} from "@/Utils"

import vueSlider from 'vue-slider-component';

Vue.use(VueScroll)

interface C extends Vue {
  data: {}[]
  rows: {}[],
  columns: string[],
  scrolly: number
  scrollx: number
  $scrollYDebounced: Function
  $scrolly: number
  $scrollFixTask: number
}

export default {
  props: ["uid", "data"],
  components: {
    vueSlider
  },
  data() {
    return {
      scrollx: 0,
      scrolly: 100,
    }
  },
  methods: {
    id(row: {}) {
      return Math.random()
    },
    on_scroll(e) {
      if(!this.$scrollYDebounced){
        this.$scrollYDebounced = () => {
          console.log(this.scrolly, this.$scrolly)
          this.scrolly = this.$scrolly;
          (<any>this.$refs["qwe"]).setValue(this.$scrolly);
        }
        this.$scrollYDebounced = throttle(this.$scrollYDebounced, 16);
        this.$scrolly = this.scrolly
      }
      if(!e.deltaY) return;
      let s = e.deltaY
      let dy = Math.abs(s) > 50 ? s : s * 25
      console.log(e,s)
      
      var y = this.$scrolly - dy / this.rows.length
      y = Math.min(100, Math.max(0, y))
      
      this.$scrolly = y

      this.$scrollYDebounced()
    }
  },
  computed: {
    columns() {
      return this.data.length ? Object.getOwnPropertyNames(this.data[0]).filter(x => x != "__ob__") : []
    },
    rows() {
      let all = (this.data || [])
        .map(obj => this.columns.map(c => obj[c]))
      return all;
    },
    row_nums(){
      let buffer = 50;
      let all = this.rows
      let start = Math.floor(Math.min( 
        all.length / 100.0 * (100 - this.scrolly),
        all.length - 1
      ))
      
      var nums = Array.apply(null, Array(buffer)).map((_, i) => start + i + 1)
      return nums
    },
    visible_rows() {
      let buffer = 50;
      let all = this.rows
      let start = Math.min( 
        all.length / 100.0 * (100 - this.scrolly),
        all.length - 1
      )
      
      var visible = all.slice(start, start + buffer)
      return visible
    },
  },
  mounted(){
    this.$scrollFixTask = setInterval(() => {
      (<any>this.$refs["qwe"]).refresh();
    }, 250);
  },
  destroyed(){
    clearInterval(this.$scrollFixTask)
  },
  updated() {
  },
  watch:{
    scrolly() {
      this.$scrolly = this.scrolly
    }
  }
} as ComponentOptions<C>
</script>

<style scoped>
/*table {
  height: 100%
}*/


/*.row {
  height: 50px;
}

.cell,
.header-cell {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.header-cell {
  width: 98px;
}

.cell-container {
  border-spacing: 2px;
}*/

.cell {
  padding: 0px;
  margin: 0px;
  height: 20px;
  width: 200px;
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
}

.rownums-table {
  width: 40px; 
}


.rownum-cell {
  padding: 0px;
  margin: 0px;
  height: 20px;
  width: 40px;
  white-space: nowrap;
  overflow: hidden;
}

.table-container {
  height: 100%;
  float: left;
  overflow: hidden;
  width: calc(100% - 50px)
}

.data-table {
  position: relative;
}

.scrollx-container {
  display: inline-block;
  height: calc(100% - 60px); 
  width: 20px; 
  float: right; 
  margin-left: 0px;
}

tbody tr:nth-child(odd) {
  background-color: #ccc;
}

th, td {
    border: 1px solid black;
    width: 100px;
}

.vue-slider-vertical .vue-slider {
  width: 20px;
}
</style>
