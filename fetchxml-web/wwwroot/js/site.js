

new Vue({
    el: "#editor",
    data: {
      input: "",
      converted: "",
      error: null
    },
    computed: {
      SQL: function() {
        return this.converted;
      },
      ErrorStatus: function() {
        return this.error;
      }
    },
    methods: {
      convert: function(e) {
        this.converted = 'Contacting server...';
        this.error = null;

        var vm = this
        axios.post('/fetchxml/convert', { text: this.input })
          .then(function (response) {
            vm.converted = response.data;
          })
          .catch(function (error) {
            console.log(error);
            vm.error = error.data;
            vm.converted = null;
          });

      }
    }
  });
