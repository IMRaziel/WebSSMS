import Vue from "vue";
import HelloComponent from "./components/Hello.vue";
import store from "./Store"

let v = new Vue({
    el: "#app",
    render: h => h(HelloComponent),
    store
});
