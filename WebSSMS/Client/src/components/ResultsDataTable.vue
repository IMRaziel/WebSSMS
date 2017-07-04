<template>
  <div style="height: 100%; overflow: hidden" @mousewheel="on_scroll">
    <div class="container" style="display: inline;">
      <div class="header" style="height: 20px"></div>
      <div style=" overflow: hidden; display: inline-block; background-color: gray; width:calc(100% - 20px); height: calc(100% - 40px)" @scroll="on_scroll">
        <table ref="table" class="display" cellspacing="0" :style="{ 'margin-left': -10 * scrollx + 'px'}">
          <thead>
            <tr class="row">
              <th :key="'col'-key" v-for="key in columns">
                <div class="cell">
                  {{ key }}
                </div>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, i) in visible_rows" class="row">
              <td v-for="f in row" class="cell-container">
                <div class="cell">
                  {{ f }}
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div style="display: inline-block; height: calc(100% - 40px); width: 20px; float: right; margin-left: 0px">
        <input step=any type="range" orient="vertical" v-model.number="scrolly"></input>
      </div>
    </div>
    <input step=any type="range" style="width: calc(100% - 20px); height: 20px; margin-top: -4px" v-model.number="scrollx"></input>
  </div>
</template>

<script lang="ts">
import Vue, { ComponentOptions } from "vue";
import { Http } from "../Utils";
import $ from "jquery";
// import  "jquery.datatables"
import 'datatables.net'
import 'datatables.net-dt/css/jquery.dataTables.css'
import 'datatables.net-fixedcolumns'
import 'datatables.net-fixedcolumns-dt/css/fixedColumns.dataTables.css'
import 'datatables.net-fixedheader'
import 'datatables.net-fixedheader-dt/css/fixedHeader.dataTables.css'
import VueScroll from "vue-scroll"

Vue.use(VueScroll)

interface C extends Vue {
  data: {}[]
  rows: {}[],
  columns: string[],
  scrolly: number
}

export default {
  props: ["uid", "data"],
  components: {
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
      console.log(e)
      var y = this.scrolly + Math.sign(e.wheelDeltaY) * (100 / this.rows.length)
      y = Math.min(100, Math.max(0, y))
      this.scrolly = y
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
  watch: {
    rows(val) {
      if (val && val.length) {
        Vue.nextTick(() =>
          setTimeout(_ => {
            let parent = (this.$refs["table"] as Element).parentNode as HTMLElement

            // $(this.$refs["table"]).DataTable({
            //   "scrollY": parent ? parent.clientHeight : 0,
            //   "scrollX": true,

            //   "searching": false,
            //   "paging": false,
            //   "ordering": false,
            //   "info": false,
            //   "fixedColumns": true,
            // } as any)
          }, 100)
        )
      }
    }
  },
  mounted() {
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
</style>
