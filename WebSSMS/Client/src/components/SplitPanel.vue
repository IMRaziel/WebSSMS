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
  direction: string
}

export default {
  props: {
    direction: {
      type: String,
      required: true,
      validator: x => x == "|" || x == "--"
    }
  },
  data() {
    return {
    }
  },
  methods: {
  },
  computed: {
  },
  mounted() {
    let children = [].slice.call(this.$el.children)
    children.forEach(el => {
      el.className += "content split split-" + (this.direction == "--" ? "vertical" : "horizontal")
    });
    Split(children, { 
      // sizes: [50, 50],
      // minSize: 100 ,
      gutterSize: 10,
      cursor: 'col-resize',
      direction: this.direction == "--" ? "vertical" : "horizontal"
    })
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
  background-color: transparent;

  background-repeat: no-repeat;
  background-position: 50%;
}

.gutter.gutter-horizontal {
  cursor: col-resize;
  background-image: url('/static/img/vertical.png');
}

.gutter.gutter-vertical {
  cursor: row-resize;
  background-image: url('/static/img/horizontal.png');
}

.split.split-horizontal, .gutter.gutter-horizontal {
  height: 100%;
  float: left;
}
</style>
