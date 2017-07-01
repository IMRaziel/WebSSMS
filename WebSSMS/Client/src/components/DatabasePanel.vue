<template>
  <MonacoEditor language="sql">
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
