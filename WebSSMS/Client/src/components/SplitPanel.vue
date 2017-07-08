<!-- src/components/Hello.vue -->
<template>
    <div style="height: 100%; width: 100%">
      <slot></slot>
      <!--<button @click="decrement">-</button>
        <button @click="testAjax">+</button>-->
    </div>
</template>

<script lang="ts">
import Split from "Split.js"
import Vue, { ComponentOptions } from "vue";

interface C extends Vue {
  direction: string,
  activateSplit: Function,
  $split: any,
  isDynamic:boolean
}

export default {
  props: {
    direction: {
      type: String,
      required: true,
      validator: x => x == "|" || x == "--",
    },
    isDynamic: {
      type: Boolean,
      default: false
    }

  },
  data() {
    return {
    }
  },
  methods: {
    activateSplit(){
      if (this.$children.length * 2 - 1 == this.$el.children.length) {
        return this.$split;
      }

      let children = [].slice.call(this.$el.children) as HTMLElement[];
      children
          .filter(x => x.className.indexOf("gutter") != -1)
          .forEach(x => {
            this.$el.removeChild(x)
          })
      if (!this.$el.children.length) return
 
      children.forEach(el => {
        let c = "content split split-" + (this.direction == "--" ? "vertical" : "horizontal")
        if(el.className.indexOf(c)==-1) el.className += c
      });

      console.log(children)
      this.$split =  Split([].slice.call(this.$el.children), {
        // sizes: [50, 50],
        // minSize: 100 ,
        gutterSize: 4,
        cursor: 'col-resize',
        direction: this.direction == "--" ? "vertical" : "horizontal"
      })
    }
  },
  destroyed(){
  },
  computed: {
  },
  updated(){
    if(!this.isDynamic) return;
    // debugger;
    if(this.$children.length * 2 - 1 != this.$el.children.length){
      if(this.$split) this.$split.destroy()
        this.$split = this.activateSplit()
    }
  },
  mounted() {
    this.activateSplit();
  }
} as ComponentOptions<C>;
</script>

<style>

.split {
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  box-sizing: border-box;

  overflow-y: hidden;
  overflow-x: hidden;
}

.content {
  border: 1px solid #C0C0C0;
  box-shadow: inset 0 1px 2px #e4e4e4;
  background-color: #fff;
}

.gutter {
  background-color: lightblue;

  background-repeat: no-repeat;
  background-position: 50%;
}

.gutter.gutter-horizontal {
  cursor: col-resize;
}

.gutter.gutter-vertical {
  cursor: row-resize;
}

.split.split-horizontal, .gutter.gutter-horizontal {
  height: 100%;
  float: left;
}
</style>
