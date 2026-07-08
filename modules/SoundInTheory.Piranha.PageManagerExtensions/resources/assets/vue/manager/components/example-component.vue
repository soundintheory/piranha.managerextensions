<template>
    <!-- Registered globally as <example-component>. Delete or rename this file for your own. -->
    <div class="example-component">
        {{ message }}
    </div>
</template>

<script>
export default {
    // Field components receive uid/model/meta; block components receive uid/toolbar/model.
    props: ["uid", "model", "meta"],
    data() {
        return { message: "Hello from example-component" };
    }
}
</script>
