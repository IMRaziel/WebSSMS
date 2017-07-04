import Vue, {} from "vue";
import HelloComponent from "./components/Hello.vue";
import store from "./Store"

import elementResizeDetectorMaker from "element-resize-detector"
var erd = elementResizeDetectorMaker({
    // @NOTE: for some reason default strategy did not work for me
    //        Chrome 48.0.2564.116, Windows 7
    strategy: 'scroll',
});



var scroll = {
    deep: true,
    update: function (model) {
        if (model.left !== undefined) this.el.scrollLeft = model.left;
        if (model.top !== undefined) this.el.scrollTop = model.top;
    },
};

var scrollModel = {
    twoWay: true,
    deep: true,
    bind: function () {
        var self = this;

        self.handler = function (ev) {
            // @NOTE: ignore whether the event was prevented or not,
            //        because scroll happens anyway
            if (ev.target === self.el) self.upcalc();
        };

        self.$on(self.el, 'scroll', self.handler);
    },
    update: function (model) {
        if (model.left !== undefined) this.el.scrollLeft = model.left;
        if (model.top !== undefined) this.el.scrollTop = model.top;

        // @NOTE: usually the property will be clipped to max
        //        and the `scroll` event will be triggered
        //        and the `model` will be adjusted in the `handler`
        // @NOTE: the only exception is: if the max is 0
        //        then setting the property clips it to 0
        //        and does not trigger the `scroll` event
        // @NOTE: so we have to manually check the actual
        //        property value and update the `model`
        if (this.el.scrollLeft !== model.left || this.el.scrollTop !== model.top) this.upcalc();
    },
    upcalc: function () {
        this.set({
            left: this.el.scrollLeft,
            top: this.el.scrollTop,
        });
    },
    unbind: function () {
        if (this.handler !== undefined) {
            this.$off(this.el, 'scroll', this.handler);
            this.handler = undefined;
        }
    },
};

var scrollSpy = {
    twoWay: true,
    bind: function () {
        var self = this;

        self.handler = function (ev) {
            // @NOTE: ignore whether the event was prevented or not,
            //        because scroll happens anyway
            if (ev.target === self.el) self.upcalc();
        };

        Vue.nextTick(function () {
            self.upcalc();
        });

        this.$on(self.el, 'scroll', self.handler);
    },
    upcalc: function () {
        this.set({
            left: this.el.scrollLeft,
            top: this.el.scrollTop,
        });
    },
    unbind: function () {
        if (this.handler !== undefined) {
            this.$off(this.el, 'scroll', this.handler);
            this.handler = undefined;
        }
    },
};

var scrollMaxSpy = {
    twoWay: true,
    bind: function () {
        var self = this;

        self.handler = function () {
            self.upcalc();
        };

        Vue.nextTick(self.handler);

        erd.listenTo(self.el, self.handler);
    },
    upcalc: function () {
        this.set({
            left: this.el.scrollWidth - this.el.clientWidth,
            top: this.el.scrollHeight - this.el.clientHeight,
        });
    },
    unbind: function () {
        erd.removeListener(this.el, this.handler);
    },
};


let v = new Vue({
    el: "#app",
    render: h => h(HelloComponent),
    store,
    directives: {
        'scroll-model': scrollModel,
        'scroll-max-spy': scrollMaxSpy,
    },
});
