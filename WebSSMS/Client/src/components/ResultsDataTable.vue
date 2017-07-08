<template>
  <div style="height: 100%; overflow: hidden" @wheel="on_scroll">
    <div class="container" style="display: inline;">
      <div style=" overflow: hidden; display: inline-block; background-color: gray; width:calc(100% - 20px); height: calc(100% - 60px)" @scroll="on_scroll">
        <div :style="{ 'margin-left': -10 * scrollx + 'px', height: '100%', width: '100%'}">
          <table ref="table" class="display" cellspacing="0">
            <thead>
              <tr >
                <th :key="'col'-key" v-for="key in columns">
                  <!--<div class="cell">-->
                    {{ key }}
                  <!--</div>-->
                </th>
              </tr>
            </thead>
            <tbody>
              <tr :key="uid + i" v-for="(row, i) in visible_rows" >
                <td :key="(uid + i) + j" v-for="(f, j) in row" >
                  <!--<div class="cell">-->
                    {{ f }}
                  <!--</div>-->
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div style="display: inline-block; height: calc(100% - 60px); width: 20px; float: right; margin-left: 0px">
        <vue-slider ref="qwe"
              height="100%" 
              :width="10"
               direction="vertical" 
               show
               tooltip="never"
              :piecewise="false"
              :process-style="{background: 'transparent'}"
              :style="{ 'display': 'inline-block', 'marginLeft': '-6px' }" 
              v-model.number="scrolly"  ></vue-slider>
      </div>
    </div>
    <vue-slider tooltip="never" step=any type="range" style="width: calc(100% - 20px); height: 20px; margin-top: -4px" v-model.number="scrollx" data-rangeslider></vue-slider>
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
    visible_rows() {
      let buffer = 50;
      let all = this.rows
      let start = Math.min( 
        all.length / 100.0 * (100 - this.scrolly),
        all.length - 1
      )
      
      var visible = all.slice(start, start + buffer)
      return visible
    }
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

<style >
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

.cell {
  padding: 0px;
  margin: 0px;
  width: 100px;
}

.header-cell {
  width: 98px;
}

.cell-container {
  border-spacing: 2px;
}*/

tbody tr:nth-child(odd) {
  background-color: #ccc;
}

.vue-slider-vertical .vue-slider {
  width: 20px;
}
</style>
