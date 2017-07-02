<template>
  <MonacoEditor language="sql" @codeChange="onCodeChange" @mounted="onMounted" 
            :changeThrottle="250" :options="{automaticLayout: true}"
  >
  </MonacoEditor>
</template>

<script lang="ts">
import Vue from "vue";
import { Http } from "../Utils";
import $ from "jquery";

import MonacoEditor from 'vue-monaco-editor'


export default Vue.extend({
  components: {
    MonacoEditor
  },
  props: ["onCodeChange", "onMounted"],
  data() {
    return {

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
