<template>
  <MonacoEditor language="sql" @codeChange="onCodeChange" @mounted="onMounted" 
            :changeThrottle="250" :options="{automaticLayout: true}" :code="test_code"
  >
  </MonacoEditor>
</template>

<script lang="ts">
import Vue from "vue";
import { Http } from "../Utils";
import $ from "jquery";

import MonacoEditor from './Monaco/Monaco.vue'


export default Vue.extend({
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
  }
});
</script>

<style>

</style>
