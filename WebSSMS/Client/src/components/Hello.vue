<!-- src/components/Hello.vue -->
<template>
    <Split direction="--">
        <div> Hello </div>
        <div> Hello2</div>
    </Split>
</template>

<script lang="ts">
import Vue from "vue";
import { Http } from "../Utils";
import $ from "jquery";
import Split from "./SplitPanel.vue"


async function test(){
    var q = await Http.get("/api/meta")
    return q;
}

export default Vue.extend({
    components: {
        Split
    },
    props: ['name', 'initialEnthusiasm'],
    data() {
        return {
            ajaxTestValue: "",
            enthusiasm: this.initialEnthusiasm,
        }
    },
    methods: {
        increment() { this.enthusiasm++; },
        decrement() {
            if (this.enthusiasm > 1) {
                this.enthusiasm--;
            }
        },
        async testAjax(){
            this.ajaxTestValue = await test(); 
        }
    },
    computed: {
        exclamationMarks(): string {
            return Array(this.enthusiasm + 1).join('!');
        }
    }
});
</script>

<style>
.greeting {
    font-size: 20px;
}
</style>
