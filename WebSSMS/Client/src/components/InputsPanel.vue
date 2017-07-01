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
import Vue from "vue";



export default Vue.extend({
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
    debugger
    console.log(Split)
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
});
</script>

<style>

</style>
