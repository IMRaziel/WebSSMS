<template>
  <MonacoEditor ref="editor" language="sql" @codeChange="onCodeChange" @mounted="onMounted" 
            :changeThrottle="250" :options="{automaticLayout: true}" :code="test_code"
  >
  </MonacoEditor>
</template>

<script lang="ts">
import Vue, { ComponentOptions } from "vue";
import { Http } from "../Utils";
import $ from "jquery";

import MonacoEditor from './Monaco/Monaco.vue'
interface C extends Vue {
  editor: any
}

export default{
  components: {
    MonacoEditor
  },
  props: ["onCodeChange", "onMounted"],
  data() {
    return {
      test_code: this.$store.state.editor.code
    }
  },
  methods: {

  },
  computed: {
  },
  mounted(){
    let self = this;
    this.$store.watch(state => state.editor._loadedCode, (newVal, oldVal) => {
      debugger
      let editor: any = self.$refs["editor"]
      editor.editor.setValue(newVal)
    })
  }
} as ComponentOptions<C>  
</script>

<style>

</style>
